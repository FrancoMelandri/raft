using System.Linq;
using System.Collections.Generic;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Constants.MessageConstants;
using static RaftCore.Node.Validations;
using static RaftCore.Node.Utils;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public class Agent : IAgent
    {
        public NodeConfiguration Configuration { get; private set; }
        private Descriptor _descriptor;
        private readonly ICluster _cluster;
        private readonly IElection _election;

        private Agent(ICluster cluster, IElection election)
        {
            _cluster = cluster;
            _election = election;
        }

        public static Agent Create(ICluster cluster, IElection election)
            => new(cluster, election);

        public Descriptor OnInitialise(NodeConfiguration nodeConfiguration)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _descriptor = new Descriptor
                {
                    CurrentTerm = INIT_TERM,
                    VotedFor = INIT_VOTED_FOR,
                    Log = INIT_LOG,
                    CommitLenght = INIT_COMMIT_LENGTH,
                    CurrentRole = INIT_STATE,
                    CurrentLeader = INIT_CURRENT_LEADER,
                    VotesReceived = INIT_VOTES_RECEIVED,
                    SentLength = INIT_SENT_LENGTH,
                    AckedLength = INIT_ACKED_LENGTH
                })
                .Map(_ => _descriptor);

        public Descriptor OnRecoverFromCrash(NodeConfiguration nodeConfiguration, Descriptor descriptor)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _descriptor = new Descriptor
                {
                    CurrentTerm = descriptor.CurrentTerm,
                    VotedFor = descriptor.VotedFor,
                    Log = descriptor.Log,
                    CommitLenght = descriptor.CommitLenght,
                    CurrentRole = INIT_STATE,
                    CurrentLeader = INIT_CURRENT_LEADER,
                    VotesReceived = INIT_VOTES_RECEIVED,
                    SentLength = INIT_SENT_LENGTH,
                    AckedLength = INIT_ACKED_LENGTH
                })
                .Map(_ => _descriptor);

        public Descriptor OnLeaderHasFailed()
            => new Descriptor
            {
                CurrentTerm = _descriptor.CurrentTerm + 1,
                VotedFor = Configuration.Id,
                Log = _descriptor.Log,
                CommitLenght = _descriptor.CommitLenght,
                CurrentRole = States.Candidate,
                CurrentLeader = _descriptor.CurrentLeader,
                VotesReceived = new[] { Configuration.Id },
                SentLength = _descriptor.SentLength,
                AckedLength = _descriptor.AckedLength
            }
                .Tee(descriptor => _descriptor = descriptor)
                .Tee(descriptor => LastEntryOrZero(descriptor.Log)
                                    .Map(_ => BuildMessage(MessageType.VoteRequest, _, GRANT))
                                    .Tee(_ => _cluster.SendBroadcastMessage(_))
                                    .Tee(_ => _election.Start()))
                .Map(_ => _descriptor);

        public Descriptor OnElectionTimeOut()
            => OnLeaderHasFailed();

        public Descriptor OnReceivedVoteRequest(VoteRequestMessage message)
            => ValidateLog(_descriptor, message)
                .Bind(_ => ValidateTerm(_descriptor, message))
                .Match(_ => ReceivedVoteRequestGrantResponse(message),
                       _ => ReceivedVoteRequestDontGrantResponse(message))
                .Map(_ => _descriptor);

        public Descriptor OnReceivedVoteResponse(VoteResponseMessage message)
            => ValidateVoteGrant(_descriptor, message)
                .Match(_ => ReceivedVoteResponseGranted(message),
                       _ => ValidateTerm(_descriptor, message)
                                .Match(_ => ReceivedVoteResponseNoGrantedUpdateDescriptor(message),
                                        _ => Unit.Default))
                .Map(_ => _descriptor);

        public Descriptor OnBroadcastMessage(Message message)
            => IsLeader(_descriptor)
                .Match(_ => HandleBroadcastMessageAsLeader(message),
                       _ => _descriptor.Tee(descriptor => _cluster.SendMessage(descriptor.CurrentLeader, message)));

        public Descriptor OnReplicateLog()
            => IsLeader(_descriptor)
                .Match(_ => _descriptor.Tee(descriptor => ReplicateLogToFollowers(descriptor)),
                       _ => _descriptor);

        private Unit ReplicateLogToFollower(Descriptor descriptor, int follower)
            => descriptor.SentLength[follower]
                .Map(length => (Length: length, PrevLogTerm: length > 0 ? descriptor.Log[length - 1].Term : 0))
                .Tee(_ =>  _cluster.SendMessage(follower, new LogRequestMessage
                                    {
                                        Type = MessageType.LogRequest,
                                        LeaderId = Configuration.Id,
                                        CurrentTerm = descriptor.CurrentTerm,
                                        StartLength = _.Length,
                                        PrevTerm = _.PrevLogTerm,
                                        CommitLength = descriptor.CommitLenght,
                                        Entries = descriptor.Log.TakeLast(descriptor.Log.Length - descriptor.SentLength[follower] - 1).ToArray()
                                    }))
                .Map(_ => Unit.Default);

        private Descriptor ReplicateLogToFollowers(Descriptor descriptor)
            => _cluster
                    .Nodes
                    .Filter(_ => _.Id != Configuration.Id)
                    .ForEach(follower => ReplicateLogToFollower(descriptor, follower.Id))
                    .Map(_ => descriptor);        

        private Descriptor HandleBroadcastMessageAsLeader(Message message)
            => (Log: _descriptor.Log.Concat(new LogEntry[] { new LogEntry { Message = message, Term = _descriptor.CurrentTerm } }).ToArray(),
                AckedLength: _descriptor.AckedLength.Tee(_ => _[Configuration.Id] = _descriptor.Log.Length + 1))
                .Map(_ => new Descriptor
                        {
                            CurrentTerm = _descriptor.CurrentTerm,
                            VotedFor = _descriptor.VotedFor,
                            Log = _.Log,
                            CommitLenght = _descriptor.CommitLenght,
                            CurrentRole = _descriptor.CurrentRole,
                            CurrentLeader = _descriptor.CurrentLeader,
                            VotesReceived = _descriptor.VotesReceived,
                            SentLength = _descriptor.SentLength,
                            AckedLength = _.AckedLength
                        })
                .Tee(_ => ReplicateLogToFollowers(_));

        private Unit ReceivedVoteResponseGranted(VoteResponseMessage message)
            => new Descriptor
                    {
                        CurrentTerm = _descriptor.CurrentTerm,
                        VotedFor = _descriptor.VotedFor,
                        Log = _descriptor.Log,
                        CommitLenght = _descriptor.CommitLenght,
                        CurrentRole = _descriptor.CurrentRole,
                        CurrentLeader = _descriptor.CurrentLeader,
                        VotesReceived = _descriptor.VotesReceived.Concat(new int[] { message.NodeId }).Distinct().ToArray(),
                        SentLength = _descriptor.SentLength,
                        AckedLength = _descriptor.AckedLength
                    }
                .Map(descriptor => ValidateVotesQuorum(descriptor, _cluster)
                                    .Match(_ => ReceivedVoteResponseGrantedPromoteAsLeader(descriptor),
                                           _ => descriptor))
                .Tee(descriptor => _descriptor = descriptor)
                .Map(_ => Unit.Default);

        private Descriptor ReceivedVoteResponseGrantedPromoteAsLeader(Descriptor descriptor)
            => new Descriptor
                    {
                        CurrentTerm = descriptor.CurrentTerm,
                        VotedFor = descriptor.VotedFor,
                        Log = descriptor.Log,
                        CommitLenght = descriptor.CommitLenght,
                        CurrentRole = States.Leader,
                        CurrentLeader = Configuration.Id,
                        VotesReceived = descriptor.VotesReceived,
                        SentLength = descriptor.SentLength,
                        AckedLength = descriptor.AckedLength
                    }
                .Tee(_ => _election.Cancel())
                .Tee(_ => ReceivedVoteResponseGrantedUpdateFollowers(_));

        private Descriptor ReceivedVoteResponseGrantedUpdateFollowers(Descriptor descriptor)
            => _cluster.Nodes
                .Filter(_ => _.Id != Configuration.Id)
                .Map(_ => (_.Id, descriptor.Log.Length))
                .Fold((SentLength: new Dictionary<int, int>(), AckedLength: new Dictionary<int, int>()),
                      (a, i) => a.Tee(_ => a.SentLength.Add(i.Id, i.Length))
                                 .Tee(_ => a.AckedLength.Add(i.Id, 0)))
                .Map(_ => (Descriptor: new Descriptor
                            {
                                CurrentTerm = descriptor.CurrentTerm,
                                VotedFor = descriptor.VotedFor,
                                Log = descriptor.Log,
                                CommitLenght = descriptor.CommitLenght,
                                CurrentRole = descriptor.CurrentRole,
                                CurrentLeader = descriptor.CurrentLeader,
                                VotesReceived = descriptor.VotesReceived,
                                SentLength = _.SentLength,
                                AckedLength = _.AckedLength
                            },
                           Followers: _.SentLength.Keys))
                .Tee(_ => _.Followers.ForEach(follower => ReplicateLogToFollower(_.Descriptor, follower)))
                .Map(_ => _.Descriptor);

        private Unit ReceivedVoteResponseNoGrantedUpdateDescriptor(VoteResponseMessage message)
            => new Descriptor
                    {
                        CurrentTerm = message.CurrentTerm,
                        VotedFor = INIT_VOTED_FOR,
                        Log = _descriptor.Log,
                        CommitLenght = _descriptor.CommitLenght,
                        CurrentRole = States.Follower,
                        CurrentLeader = _descriptor.CurrentLeader,
                        VotesReceived = _descriptor.VotesReceived,
                        SentLength = _descriptor.SentLength,
                        AckedLength = _descriptor.AckedLength
                    }
                .Tee(descriptor => _descriptor = descriptor)
                .Map(_ => _election.Cancel());

        private Unit ReceivedVoteRequestGrantResponse(VoteRequestMessage message)
            => new Descriptor
                    {
                        CurrentTerm = message.CurrentTerm,
                        VotedFor = message.NodeId,
                        Log = _descriptor.Log,
                        CommitLenght = _descriptor.CommitLenght,
                        CurrentRole = States.Follower,
                        CurrentLeader = _descriptor.CurrentLeader,
                        VotesReceived = _descriptor.VotesReceived,
                        SentLength = _descriptor.SentLength,
                        AckedLength = _descriptor.AckedLength
                    }
                .Tee(descriptor => _descriptor = descriptor)
                .Map(descriptor => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, descriptor.CurrentTerm, GRANT)));

        private Unit ReceivedVoteRequestDontGrantResponse(VoteRequestMessage message)
            => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, _descriptor.CurrentTerm, NO_GRANT));

        private Message BuildMessage(MessageType type, int lastTerm, bool granted)
            => type switch
            {
                MessageType.VoteRequest => new VoteRequestMessage
                {
                    Type = MessageType.VoteRequest,
                    NodeId = Configuration.Id,
                    CurrentTerm = _descriptor.CurrentTerm,
                    LogLength = _descriptor.Log.ToOption().Map(_ => _.Length).OnNone(0),
                    LastTerm = lastTerm
                },
                MessageType.VoteResponse => new VoteResponseMessage
                {
                    Type = MessageType.VoteResponse,
                    NodeId = Configuration.Id,
                    CurrentTerm = _descriptor.CurrentTerm,
                    Granted = granted
                },
                _ => Message.Empty
            };
    }
}

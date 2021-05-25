using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.Validations;
using static RaftCore.Node.Utils;

namespace RaftCore.Node
{
    public class Agent
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
                .Tee(_ => _._descriptor = new Descriptor
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
                .Tee(_ => _._descriptor = new Descriptor
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
                .Map(_ => _._descriptor);

        public Descriptor OnLedaerHasFailed()
            => new Descriptor
                {
                    CurrentTerm = _descriptor.CurrentTerm + 1,
                    VotedFor = Configuration.Id,
                    Log = _descriptor.Log,
                    CommitLenght = _descriptor.CommitLenght,
                    CurrentRole = States.Candidate,
                    CurrentLeader = _descriptor.CurrentLeader,
                    VotesReceived = Configuration.Id,
                    SentLength = _descriptor.SentLength,
                    AckedLength = _descriptor.AckedLength
                }
                .Tee(descriptor => _descriptor = descriptor)
                .Tee(descriptor => LastEntryOrZero(descriptor.Log)
                                    .Map(_ => BuildMessage(MessageType.VoteRequest, _, true))
                                    .Tee(_ => _cluster.SendBroadcastMessage(_))
                                    .Tee(_ => _election.Start()))
                .Map(_ => _descriptor);

        public Descriptor OnElectionTimeOut()
            => OnLedaerHasFailed();

        public Descriptor OnReceivedVoteRequest(VoteRequestMessage message)
            => ValidateLog(_descriptor, message)
                .Bind(_ => ValidateTerm(_descriptor, message))
                .Match(
                    _ => new Descriptor
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
                            .Map(descriptor => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, descriptor.CurrentTerm, true))),
                    _ => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, _descriptor.CurrentTerm, false))
                )
                .Map(_ => _descriptor);

        public Unit OnReceivedVoteResponse(VoteResponseMessage message)
            => Unit.Default;

        private Message BuildMessage(MessageType type, int lastTerm, bool granted)
            => type switch
            {
                MessageType.VoteRequest => new VoteRequestMessage
                {
                    Type = MessageType.VoteRequest,
                    NodeId = Configuration.Id,
                    CurrentTerm = _descriptor.CurrentTerm,
                    LogLength = _descriptor.Log.Length,
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

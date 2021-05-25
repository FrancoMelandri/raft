﻿using RaftCore.Cluster;
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
        public Descriptor Descriptor { get; private set; }

        private readonly ICluster _cluster;
        private readonly IElection _election;

        private Agent(ICluster cluster, IElection election)
        {
            _cluster = cluster;
            _election = election;
        }

        public static Agent Create(ICluster cluster, IElection election)
            => new(cluster, election);

        public Agent OnInitialise(NodeConfiguration nodeConfiguration)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _.Descriptor = new Descriptor
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
                });

        public Agent OnRecoverFromCrash(NodeConfiguration nodeConfiguration, Descriptor descriptor)
            => this
                .Tee(_ => _.Configuration = nodeConfiguration)
                .Tee(_ => _.Descriptor = new Descriptor
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
                });

        public void OnLedaerHasFailed()
            => new Descriptor
                {
                    CurrentTerm = Descriptor.CurrentTerm + 1,
                    VotedFor = Configuration.Id,
                    Log = Descriptor.Log,
                    CommitLenght = Descriptor.CommitLenght,
                    CurrentRole = States.Candidate,
                    CurrentLeader = Descriptor.CurrentLeader,
                    VotesReceived = Configuration.Id,
                    SentLength = Descriptor.SentLength,
                    AckedLength = Descriptor.AckedLength
                }
                .Tee(descriptor => Descriptor = descriptor)
                .Tee(descriptor => LastEntryOrZero(Descriptor.Log)
                                    .Map(_ => BuildMessage(MessageType.VoteRequest, _, true))
                                    .Tee(_ => _cluster.SendBroadcastMessage(_))
                                    .Tee(_ => _election.Start()));

        public void OnElectionTimeOut()
            => OnLedaerHasFailed();

        public Unit OnReceivedVoteRequest(VoteRequestMessage message)
            => ValidateLog(Descriptor, message)
                .Bind(_ => ValidateTerm(Descriptor, message))
                .Match(
                    _ => new Descriptor
                            {
                                CurrentTerm = message.CurrentTerm,
                                VotedFor = message.NodeId,
                                Log = Descriptor.Log,
                                CommitLenght = Descriptor.CommitLenght,
                                CurrentRole = States.Follower,
                                CurrentLeader = Descriptor.CurrentLeader,
                                VotesReceived = Descriptor.VotesReceived,
                                SentLength = Descriptor.SentLength,
                                AckedLength = Descriptor.AckedLength
                            }
                            .Tee(descriptor => Descriptor = descriptor)
                            .Map(_ => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, Descriptor.CurrentTerm, true))),
                    _ => _cluster.SendMessage(message.NodeId, BuildMessage(MessageType.VoteResponse, Descriptor.CurrentTerm, false))
                );

        public Unit OnReceivedVoteResponse(VoteResponseMessage message)
            => Unit.Default;

        private Message BuildMessage(MessageType type, int lastTerm, bool inFavour)
            => type switch
            {
                MessageType.VoteRequest => new VoteRequestMessage
                {
                    Type = MessageType.VoteRequest,
                    NodeId = Configuration.Id,
                    CurrentTerm = Descriptor.CurrentTerm,
                    LogLength = Descriptor.Log.Length,
                    LastTerm = lastTerm
                },
                MessageType.VoteResponse => new VoteResponseMessage
                {
                    Type = MessageType.VoteResponse,
                    NodeId = Configuration.Id,
                    CurrentTerm = Descriptor.CurrentTerm,
                    InFavour = inFavour
                },
                _ => Message.Empty
            };
    }
}

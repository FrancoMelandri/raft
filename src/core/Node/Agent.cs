using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public class Agent
    {
        public NodeConfiguration Configuration { get; private set; }
        public Descriptor Descriptor { get; private set; }

        private readonly ICluster _cluster;

        private Agent(ICluster cluster)
        {
            _cluster = cluster;
        }

        public static Agent Create(ICluster cluster)
            => new(cluster);

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
                                    .Do(_ => _cluster.SendMessage(_)));

        public void OnElectionTimeOut()
            => OnLedaerHasFailed();

        public Unit OnReceivedVotedRequest(VoteRequestMessage message)
            => ValidateLogTerm(message)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(message)))
                .Match(
                    _ => _cluster.SendMessage(BuildMessage(MessageType.VoteResponse, 0, true)),
                    _ => _cluster.SendMessage(BuildMessage(MessageType.VoteResponse, 0, false))
                );

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


        private Either<string, int> ValidateLogTerm(VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(Descriptor.Log) ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private Either<string, int> ValidateLogLength(VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(Descriptor.Log) &&
               message.LogLength >= Descriptor.Log.Length ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private static int LastEntryOrZero(LogEntry[] Log)
            => Log
                .ToOption(_ => _.Length == 0)
                .Map(_ => _[^1].Term)
                .OnNone(0);
    }
}

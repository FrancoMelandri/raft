using RaftCore.Cluster;
using RaftCore.Models;
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
                .Tee(descriptor => descriptor.Log
                                .ToOption(_ => _.Length == 0)
                                .Map(_ => Descriptor.Log[^1].Term)
                                .OnNone(0)
                                .Map(_ => BuildMessage(MessageType.VoteRequest, _))
                                .Do(_ => _cluster.SendMessage(_)));

        public void OnElectionTimeOut()
            => OnLedaerHasFailed();

        private Message BuildMessage(MessageType type, int lastTerm)
            => new()
            {
                Type = type,
                NodeId = Configuration.Id,
                CurrentTerm = Descriptor.CurrentTerm,
                LogLength = Descriptor.Log.Length,
                LastTerm = lastTerm
            };
    }
}

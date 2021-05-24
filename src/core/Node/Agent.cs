using RaftCore.Models;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public class Agent
    {
        public NodeConfiguration Configuration { get; private set; }
        public Descriptor Descriptor { get; private set; }

        private Agent() { }

        public static Agent OnInitialise(NodeConfiguration nodeConfiguration)
            => new()
            {
                Configuration = nodeConfiguration,
                Descriptor = new Descriptor 
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
                }
            };

        public static Agent OnRecoverFromCrash(NodeConfiguration nodeConfiguration, Descriptor descriptor)
            => new()
            {
                Configuration = nodeConfiguration,
                Descriptor = new Descriptor
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
                }
            };
    }
}

using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public class Agent
    {
        public Descriptor Descriptor { get; private set; }

        private Agent() { }

        public static Agent Initialise()
            => new()
            {
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
    }
}

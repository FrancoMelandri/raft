using RaftCore.Models;

namespace RaftCore.Node
{
    public struct Descriptor
    {
        public int CurrentTerm { get; set; }
        public int VotedFor { get; set; }
        public LogEntry[] Log { get; set; }
        public int CommitLenght { get; set; }
        public States CurrentRole { get; set; }
        public int CurrentLeader { get; set; }
        public int[] VotesReceived { get; set; }
        public object[] SentLength { get; set; }
        public object[] AckedLength { get; set; }
    }
}

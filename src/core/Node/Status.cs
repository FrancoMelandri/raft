using RaftCore.Models;
using System.Collections.Generic;

namespace RaftCore.Node
{
    public struct Status
    {
        public int CurrentTerm { get; set; }
        public int VotedFor { get; set; }
        public LogEntry[] Log { get; set; }
        public int CommitLenght { get; set; }
        public States CurrentRole { get; set; }
        public int CurrentLeader { get; set; }
        public int[] VotesReceived { get; set; }
        public IDictionary<int, int> SentLength { get; set; }
        public IDictionary<int, int> AckedLength { get; set; }
    }
}

using RaftCore.Models;
using RaftCore.Node;
using System;
using System.Collections.Generic;

namespace RaftCore.Constants
{
    public static class NodeConstants
    {
        public static readonly int INIT_TERM = 0;
        public static readonly int INIT_VOTED_FOR = -1;
        public static readonly LogEntry[] INIT_LOG = Array.Empty<LogEntry>();
        public static readonly int INIT_COMMIT_LENGTH = 0;
        public static readonly States INIT_STATE = States.Follower;
        public static readonly int INIT_CURRENT_LEADER = -1;
        public static readonly int[] INIT_VOTES_RECEIVED = Array.Empty<int>();
        public static readonly IDictionary<int, int> INIT_SENT_LENGTH = new Dictionary<int, int>();
        public static readonly IDictionary<int, int> INIT_ACKED_LENGTH = new Dictionary<int, int>();
    }
}

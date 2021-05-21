
using RaftCore.Node;
using System;

namespace RaftCore.Constants
{
    public static class NodeConstants
    {
        public static readonly int INIT_TERM = 0;
        public static readonly int INIT_VOTED_FOR = -1;
        public static readonly object[] INIT_LOG = Array.Empty<object>();
        public static readonly int INIT_COMMIT_LENGTH = 0;
        public static readonly States INIT_STATE = States.Follower;
        public static readonly int INIT_CURRENT_LEADER = -1;
        public static readonly object INIT_VOTES_RECEIVED = new();
        public static readonly object[] INIT_SENT_LENGTH = Array.Empty<object>();
        public static readonly object[] INIT_ACKED_LENGTH = Array.Empty<object>();
    }
}

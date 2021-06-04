﻿using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Constants
{
    [ExcludeFromCodeCoverage]
    public static class Logs
    {
        public static readonly string NODE_INITIALISE = "Node initialisation";
        public static readonly string NODE_INITIALISE_WITH_STATUS = "Node initialisation with status";

        public static readonly string ELECTION_TIMEOUT = "Election timeout";
        public static readonly string LEADER_HAS_FAILED = "Leader has failed";

        public static readonly string LOG_TRUNCATED = "Log was trucated";
        public static readonly string LOG_APPEND_ENTRIES = "Append new entries to log";
        public static readonly string NOTIFY_TO_APPLICATION = "Notify to application";
    }
}
using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Constants
{
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static readonly Error Empty = new("0", "");
        public static readonly Error TermIsNotGreater = new("LR-0001", "term-is-not-greater");
        public static readonly Error LengthIsNotOk = new("LR-0002", "message-length-is-not-ok");
        public static readonly Error TermIsNotOk = new("LR-0003", "message-term-is-not-ok");
        public static readonly Error CurrentTermNotOk = new("LR-0004", "current-term-is-not-ok");
        public static readonly Error EntriesLogLengthNotOk = new("LR-0005", "entries-length-not-ok");
        public static readonly Error EntrieTermIsNotOk = new("LR-0006", "entries-term-not-ok");
        public static readonly Error TermNotEqual = new("LR-0007", "term-is-not-equal");
        public static readonly Error NotALeader = new("LR-0008", "node-is-not-leader");
        public static readonly Error LogNotSuccess = new("LR-0009", "log-response-not-success");
        public static readonly Error SentLegthIsWrong = new("LR-0010", "sent-length-is-wrong");

        public static readonly Error LastTermAndLogAreWrong = new("VL-0001", "last-term-and-log-are-wrong");
        public static readonly Error CurrentTermAndVotedFroreWrong = new("VL-0002", "current-term-and-votedfor-are-wrong");

        public static readonly Error VoteIsNotValid = new("VR-0001", "vote-is-not-valid");
        public static readonly Error TermIsNotValid = new("VR-0002", "term-is-not-valid");
        public static readonly Error QuorumNotReached = new("VR-0003", "quorum-not-reached");

        public static readonly Error ElectionObserverNotRegitsred = new("EL-0001", "election-observer-not-registered");
    }
}

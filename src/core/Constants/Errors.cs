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
    }
}

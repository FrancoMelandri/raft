using RaftCore.Models;
using TinyFp.Extensions;

namespace RaftCore.Node
{
    public static class Utils
    {
        public static int LastEntryOrZero(LogEntry[] Log)
            => Log
                .ToOption(_ => _.Length == 0)
                .Map(_ => _[^1].Term)
                .OnNone(0);
    }
}

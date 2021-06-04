using RaftCore.Adapters;
using System.Diagnostics.CodeAnalysis;
using TinyFp;

namespace Raft.Loggers
{
    [ExcludeFromCodeCoverage]
    public class EmptyLogger : ILogger
    {
        public Unit Error(string message)
            => Unit.Default;

        public Unit Information(string message)
            => Unit.Default;
    }
}

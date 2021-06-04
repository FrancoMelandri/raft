using RaftCore.Adapters;
using System.Diagnostics.CodeAnalysis;
using TinyFp;
using TinyFp.Extensions;
using static System.Diagnostics.Trace;

namespace Raft.Loggers
{
    [ExcludeFromCodeCoverage]
    public class ConsoleLogger : ILogger
    {
        private Unit Log(string message)
            => Unit.Default
                .Tee(_ => WriteLine(message));

        public Unit Error(string message)
            => Log(message);

        public Unit Information(string message)
            => Log(message);
    }
}

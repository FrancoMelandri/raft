using RaftCore.Adapters;
using System.Diagnostics.CodeAnalysis;
using TinyFp;

namespace Raft.Node
{
    [ExcludeFromCodeCoverage]
    public class TcpMessageListener : IMessageListener
    {
        public Unit Start()
            => Unit.Default;

        public Unit Stop()
            => Unit.Default;
    }
}

using RaftCore.Adapters;
using RaftCore.Models;
using System.Diagnostics.CodeAnalysis;
using TinyFp;

namespace Raft.Cluster
{
    [ExcludeFromCodeCoverage]
    public class TcpMessageSender : IMessageSender
    {
        public Unit SendMessage(int nodeId, Message message)
            => Unit.Default;
    }
}

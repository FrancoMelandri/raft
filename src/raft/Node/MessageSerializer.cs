using RaftCore.Adapters;
using RaftCore.Models;

namespace Raft.Node
{
    public class MessageSerializer : IMessageSerializer
    {
        public virtual Message Deserialize(int type, string message)
            => new(){ Type = MessageType.None };
    }
}

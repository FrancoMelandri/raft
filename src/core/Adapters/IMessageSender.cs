using RaftCore.Models;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IMessageSender
    {
        Unit SendMessage(int nodeId, Message message);
    }
}

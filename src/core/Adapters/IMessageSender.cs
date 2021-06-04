using RaftCore.Models;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IMessageSender
    {
        Unit Start(int nodeId);
        Unit Stop();
        Unit SendMessage(Message message);
    }
}

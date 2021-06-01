using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface IMessageObserver
    {
        Unit NotifyMessage(Message message);
    }
}

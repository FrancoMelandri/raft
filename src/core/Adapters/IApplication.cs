using RaftCore.Models;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IApplication
    {
        Unit NotifyMessage(Message message);
    }
}

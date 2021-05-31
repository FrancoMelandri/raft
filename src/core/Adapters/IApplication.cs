using RaftCore.Models;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IApplication
    {
        Unit Initialise();
        Unit Deinitialise();
        Unit NotifyMessage(Message message);
    }
}

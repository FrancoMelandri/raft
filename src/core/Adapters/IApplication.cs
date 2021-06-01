using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IApplication
    {
        Unit Initialise(ILocalNode localNode);
        Unit Deinitialise(ILocalNode localNode);
        Unit NotifyMessage(Message message);
    }
}

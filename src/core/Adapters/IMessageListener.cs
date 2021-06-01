using RaftCore.Cluster;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IMessageListener
    {
        Unit Start(IMessageObserver messageObserver);
        Unit Stop();
    }
}

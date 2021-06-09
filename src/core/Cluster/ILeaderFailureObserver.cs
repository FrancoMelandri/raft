using TinyFp;

namespace RaftCore.Cluster
{
    public interface ILeaderFailureObserver
    {
        Unit NotifyFLeaderailure();
    }
}

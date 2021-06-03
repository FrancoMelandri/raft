using RaftCore.Cluster;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface ILeaderFailureDetector
    {
        Unit Start(ILeaderFailureObserver leaderFailureObserver);
        Unit Stop();
        Unit Reset();
    }
}

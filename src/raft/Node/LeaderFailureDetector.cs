using RaftCore.Adapters;
using RaftCore.Cluster;
using System.Threading;
using TinyFp;
using TinyFp.Extensions;

namespace Raft.Node
{
    public class LeaderFailureDetector : ILeaderFailureDetector
    {
        private ILeaderFailureObserver _leaderFailureObserver;
        private readonly LocalNodeConfiguration _localNodeConfiguration;
        private readonly Timer _timer;

        public LeaderFailureDetector(LocalNodeConfiguration localNodeConfiguration)
        {
            _localNodeConfiguration = localNodeConfiguration;
            _timer = new Timer(TimeoutCallback,
                               null,
                               Timeout.Infinite,
                               Timeout.Infinite);
        }

        public Unit Reset()
            => Unit.Default
                .Tee(_ => _timer.Change(_localNodeConfiguration.LeaderFailureTimeout, Timeout.Infinite));

        public Unit Start(ILeaderFailureObserver leaderFailureObserver)
            => Unit.Default
                .Tee(_ => _leaderFailureObserver = leaderFailureObserver)
                .Tee(_ => _timer.Change(_localNodeConfiguration.LeaderFailureTimeout, Timeout.Infinite));

        public Unit Stop()
            => Unit.Default
                .Tee(_ => _timer.Change(Timeout.Infinite, Timeout.Infinite));

        private void TimeoutCallback(object? state)
            => _leaderFailureObserver.NotifyFailure();
    }
}

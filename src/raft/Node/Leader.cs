using RaftCore.Node;
using System.Threading;
using TinyFp;
using TinyFp.Extensions;

namespace Raft.Node
{
    public class Leader : ILeader
    {
        private readonly LocalNodeConfiguration _localNodeConfiguration;
        private readonly Timer _timer;
        private IAgent _agent;

        public Leader(LocalNodeConfiguration localNodeConfiguration)
        {
            _localNodeConfiguration = localNodeConfiguration;
            _timer = new Timer(TimerIntervalCallback,
                               null,
                               Timeout.Infinite,
                               Timeout.Infinite);
        }

        public Unit Stop()
            => Unit.Default
                .Tee(_ => _timer.Change(Timeout.Infinite, Timeout.Infinite));

        public Unit Start(IAgent agent)
            => Unit.Default
                .Tee(_ => _agent = agent)
                .Tee(_ => _timer.Change(_localNodeConfiguration.LeaderReplicateLogInterval, 
                                        _localNodeConfiguration.LeaderReplicateLogInterval));

        private void TimerIntervalCallback(object? state)
            => _agent.OnReplicateLog();
    }
}

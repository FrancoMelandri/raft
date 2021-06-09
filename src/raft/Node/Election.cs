using RaftCore.Adapters;
using RaftCore.Node;
using System.Threading;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.Errors;

namespace Raft.Node
{
    public class Election : IElection
    {
        private IElectionObserver _electionObserver;
        private readonly LocalNodeConfiguration _localNodeConfiguration;
        private readonly Timer _timer;
        private readonly ILogger _logger;

        public Election(LocalNodeConfiguration localNodeConfiguration,
                        ILogger logger)
        {
            _localNodeConfiguration = localNodeConfiguration;
            _logger = logger;
            _timer = new Timer(TimeoutCallback,
                               null,
                               Timeout.Infinite,
                               Timeout.Infinite);
        }

        public Unit Stop()
            => Unit.Default
                .Tee(_ => _timer.Change(Timeout.Infinite, Timeout.Infinite));

        public Unit Start()
            => Unit.Default
                .Tee(_ => _timer.Change(_localNodeConfiguration.ElectionTimeout,
                                        _localNodeConfiguration.ElectionTimeout));

        public Unit RegisterObserver(IElectionObserver electionObserver)
            => Unit.Default
                .Tee(_ => _electionObserver = electionObserver);

        private void TimeoutCallback(object? state)
            => _electionObserver
                .ToOption()
                .Match(_ => _.NotifyElectionTimeout(),
                       () => _logger.Error($"{ElectionObserverNotRegitsred.Code}: {ElectionObserverNotRegitsred.Description}"));
    }
}

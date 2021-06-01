using Microsoft.Extensions.Logging;
using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;

namespace RaftApplication.Services.Application
{
    public class ExampleApplication : IApplication
    {
        private const string STARTED = "Raft application started.";
        private const string STOPPED = "Raft application stopped.";
        private const string MESSAGE = "Raft application message: ";

        private readonly ClusterConfiguration _clusterConfiguration;
        private readonly LocalNodeConfiguration _nodeConfiguration;
        private readonly ILogger<ExampleApplication> _logger;

        public ExampleApplication(ClusterConfiguration clusterConfiguration,
                                  LocalNodeConfiguration nodeConfiguration,
                                  ILogger<ExampleApplication> logger)
        {
            _clusterConfiguration = clusterConfiguration;
            _nodeConfiguration = nodeConfiguration;
            _logger = logger;
        }

        public Unit Deinitialise(ILocalNode localNode)
            => localNode
                .Initialise()
                .Tee(_ => _logger.LogInformation(STOPPED));

        public Unit Initialise(ILocalNode localNode)
            => localNode
                .Initialise()
                .Tee(_ => _logger.LogInformation(STARTED));

        public Unit NotifyMessage(Message message)
            => Unit.Default
                .Tee(_ => _logger.LogInformation($"{MESSAGE} {message.Type}"));
    }
}

using RaftCore.Models;
using TinyFp;

namespace RaftApplication.Services.Application
{
    public class ExampleApplication : IExampleApplication
    {
        private readonly ClusterConfiguration _clusterConfiguration;
        private readonly NodeConfiguration _nodeConfiguration;

        public ExampleApplication(ClusterConfiguration clusterConfiguration,
                                  NodeConfiguration nodeConfiguration)
        {
            _clusterConfiguration = clusterConfiguration;
            _nodeConfiguration = nodeConfiguration;
        }

        public Unit Deinitialise()
            => Unit.Default;

        public Unit Initialise()
            => Unit.Default;

        public Unit NotifyMessage(Message message)
            => Unit.Default;
    }
}

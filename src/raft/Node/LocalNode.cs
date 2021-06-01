using RaftCore.Cluster;
using RaftCore.Node;
using TinyFp;
using TinyFp.Extensions;
using RaftCore.Adapters;

namespace Raft.Node
{
    public class LocalNode : ILocalNode
    {
        private readonly LocalNodeConfiguration _nodeConfiguration;
        private readonly IAgent _agent;
        private readonly IStatusRepository _statusRepository;

        public int Id => _nodeConfiguration.Id;

        public LocalNode(LocalNodeConfiguration nodeConfiguration,
                         IAgent agent,
                         IStatusRepository statusRepository)
        {
            _nodeConfiguration = nodeConfiguration;
            _agent = agent;
            _statusRepository = statusRepository;
        }

        public Unit Initialise()
            => _statusRepository
                .LoadStatus()
                .Match(status => _agent.OnInitialise(_nodeConfiguration, status),
                       () => _agent.OnInitialise(_nodeConfiguration))
                .Map(_ => Unit.Default);

        public Unit Deinitialise()
            => _statusRepository
                .SaveStatus(_agent.CurrentStatus())
                .OnNone(Unit.Default);
    }
}

using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp.Extensions;

namespace RaftCore.Node
{
    public partial class Agent : IAgent
    {
        private Status _status;
        private readonly ICluster _cluster;
        private readonly IElection _election;
        private readonly IApplication _application;
        private LocalNodeConfiguration _localNodeConfiguration;

        public Agent(ICluster cluster, 
                     IElection election,
                     IApplication application)
        {
            _cluster = cluster;
            _election = election;
            _application = application;
        }

        public static Agent Create(ICluster cluster, 
                                   IElection election,
                                   IApplication application)
            => new(cluster, election, application);

        public Status CurrentStatus()
            => new Status()
                .Tee(status => status = _status);
    }
}

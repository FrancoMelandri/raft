using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;

namespace RaftCore.Node
{
    public partial class Agent : IAgent
    {
        private LocalNodeConfiguration _configuration;
        private Status _status;
        private readonly ICluster _cluster;
        private readonly IElection _election;
        private readonly IApplication _application;

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
    }
}

using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;

namespace RaftCore.Node
{
    public partial class Agent : IAgent
    {
        private NodeConfiguration _configuration;
        private Status _descriptor;
        private readonly ICluster _cluster;
        private readonly IElection _election;
        private readonly IApplication _application;

        private Agent(ICluster cluster, 
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

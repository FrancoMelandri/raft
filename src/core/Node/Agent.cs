using RaftCore.Cluster;
using RaftCore.Models;

namespace RaftCore.Node
{
    public partial class Agent : IAgent
    {
        private NodeConfiguration _configuration;
        private Descriptor _descriptor;
        private readonly ICluster _cluster;
        private readonly IElection _election;

        private Agent(ICluster cluster, IElection election)
        {
            _cluster = cluster;
            _election = election;
        }

        public static Agent Create(ICluster cluster, IElection election)
            => new(cluster, election);
    }
}

using RaftCore.Cluster;
using RaftCore.Models;

namespace Raft.Cluster
{
    public class ClusterNode : IClusterNode
    {
        private readonly ClusterNodeConfiguration _clusterNodeConfiguration;

        public int Id => _clusterNodeConfiguration.Id;

        public ClusterNode(ClusterNodeConfiguration clusterNodeConfiguration)
        {
            _clusterNodeConfiguration = clusterNodeConfiguration;
        }
    }
}

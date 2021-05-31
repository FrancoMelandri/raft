using RaftCore.Cluster;
using RaftCore.Models;

namespace Raft.Node
{
    public class ClusterNode : INode
    {
        public int Id { get; private set; }

        public ClusterNode(NodeConfiguration nodeConfiguration)
        {
            Id = nodeConfiguration.Id;
        }
    }
}

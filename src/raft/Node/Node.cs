using RaftCore.Cluster;
using RaftCore.Models;

namespace Raft.Node
{
    public class Node : INode
    {
        public int Id { get; private set; }

        public Node(NodeConfiguration nodeConfiguration)
        {
            Id = nodeConfiguration.Id;
        }
    }
}

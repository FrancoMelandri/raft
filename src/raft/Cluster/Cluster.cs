using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;

namespace Raft.Cluster
{
    public class Cluster : ICluster
    {
        public IClusterNode[] Nodes { get; private set; }

        public Cluster(IClusterNode[] nodes)
        {
            Nodes = nodes;
        }

        public Unit SendBroadcastMessage(Message message)
            => Unit.Default;        

        public Unit SendMessage(int nodeId, Message message)
            => Unit.Default;
    }
}

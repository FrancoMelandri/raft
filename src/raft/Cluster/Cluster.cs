using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;

namespace Raft.Cluster
{
    public class Cluster : ICluster
    {
        public INode[] Nodes { get; private set; }

        public Cluster(INode[] nodes)
        {
            Nodes = nodes;
        }

        public Unit SendBroadcastMessage(Message message)
            => Unit.Default;        

        public Unit SendMessage(int nodeId, Message message)
            => Unit.Default;
    }
}

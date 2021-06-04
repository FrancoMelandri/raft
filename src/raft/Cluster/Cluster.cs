using RaftCore.Cluster;
using RaftCore.Models;
using System.Linq;
using TinyFp;
using TinyFp.Extensions;

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
            => Unit.Default
                .Tee(_ => Nodes.ForEach(node => node.SendMessage(message)));

        public Unit SendMessage(int nodeId, Message message)
            => Unit.Default
                .Tee(_ => Nodes
                            .FirstOrDefault(node => node.Id == nodeId)
                            .ToOption()
                            .OnSome(node => node.SendMessage(message)));
    }
}

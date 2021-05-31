using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface ICluster
    {
        IClusterNode[] Nodes { get; }
        Unit SendBroadcastMessage(Message message);
        Unit SendMessage(int nodeId, Message message);
    }
}

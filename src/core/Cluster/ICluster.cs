using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface ICluster
    {
        INode[] Nodes { get; }
        Unit SendBroadcastMessage(Message message);
        Unit SendMessage(int nodeId, Message message);
    }
}

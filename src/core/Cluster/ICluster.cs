using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface ICluster
    {
        Unit SendBroadcastMessage(Message message);
        Unit SendMessage(int nodeId, Message message);
    }
}

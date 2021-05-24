using RaftCore.Models;

namespace RaftCore.Cluster
{
    public interface ICluster
    {
        void SendMessage(Message message);
    }
}

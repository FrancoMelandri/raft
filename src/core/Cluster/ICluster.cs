using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface ICluster
    {
        Unit SendMessage(Message message);
    }
}

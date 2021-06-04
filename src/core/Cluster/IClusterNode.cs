using RaftCore.Models;
using TinyFp;

namespace RaftCore.Cluster
{
    public interface IClusterNode
    {
        int Id { get; }
        Unit SendMessage(Message message);
    }
}

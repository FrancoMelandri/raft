using TinyFp;

namespace RaftCore.Cluster
{
    public interface INode
    {
        int Id { get;  }
        Unit Initialise();
    }
}

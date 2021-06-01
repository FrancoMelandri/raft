using TinyFp;

namespace RaftCore.Cluster
{

    public interface ILocalNode
    {
        int Id { get; }
        Unit Initialise();
        Unit Deinitialise();
    }
}

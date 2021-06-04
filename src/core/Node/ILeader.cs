using TinyFp;

namespace RaftCore.Node
{
    public interface ILeader
    {
        Unit Start(IAgent agent);
        Unit Stop();
    }
}

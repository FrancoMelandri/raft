using TinyFp;

namespace RaftCore.Node
{
    public interface IElection
    {
        Unit Start();
        Unit Cancel();
    }
}

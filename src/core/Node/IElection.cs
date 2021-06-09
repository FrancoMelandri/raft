using TinyFp;

namespace RaftCore.Node
{
    public interface IElection
    {
        Unit RegisterObserver(IElectionObserver electionObserver);
        Unit Start();
        Unit Stop();
    }
}

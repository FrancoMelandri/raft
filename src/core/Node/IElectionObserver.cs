using TinyFp;

namespace RaftCore.Node
{
    public interface IElectionObserver
    {
        Unit NotifyElectionTimeout();
    }
}

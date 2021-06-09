using RaftCore.Node;
using TinyFp;

namespace Raft.Node
{
    public class Election : IElection
    {
        private IElectionObserver _electionObserver;

        public Unit Stop()
            => Unit.Default;

        public Unit Start()
            => Unit.Default;

        public Unit RegisterObserver(IElectionObserver electionObserver)
            => Unit.Default;
    }
}

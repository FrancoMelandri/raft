using RaftCore.Node;
using TinyFp;

namespace Raft.Node
{
    public class Election : IElection
    {
        public Unit Cancel()
            => Unit.Default;

        public Unit Start()
            => Unit.Default;
    }
}

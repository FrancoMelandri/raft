using TinyFp.Extensions;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReplicateLog()
            => IsLeader(_status)
                .Match(_ => _status.Tee(s => ReplicateLogToFollowers(s)),
                       _ => _status);

        private Status ReplicateLogToFollowers(Status status)
            => _cluster
                .Nodes
                .Filter(_ => _.Id != _localNodeConfiguration.Id)
                .ForEach(follower => ReplicateLog(status, follower.Id))
                .Map(_ => status);
    }
}

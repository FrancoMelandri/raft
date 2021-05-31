using TinyFp.Extensions;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReplicateLog()
            => IsLeader(_descriptor)
                .Match(_ => _descriptor.Tee(descriptor => ReplicateLogToFollowers(descriptor)),
                       _ => _descriptor);

        private Status ReplicateLogToFollowers(Status descriptor)
            => _cluster
                .Nodes
                .Filter(_ => _.Id != _configuration.Id)
                .ForEach(follower => ReplicateLog(descriptor, follower.Id))
                .Map(_ => descriptor);
    }
}

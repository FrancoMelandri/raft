using TinyFp.Extensions;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReplicateLog()
            => IsLeader(_descriptor)
                .Match(_ => _descriptor.Tee(descriptor => ReplicateLogToFollowers(descriptor)),
                       _ => _descriptor);

        private Descriptor ReplicateLogToFollowers(Descriptor descriptor)
            => _cluster
                .Nodes
                .Filter(_ => _.Id != _configuration.Id)
                .ForEach(follower => ReplicateLog(descriptor, follower.Id))
                .Map(_ => descriptor);
    }
}

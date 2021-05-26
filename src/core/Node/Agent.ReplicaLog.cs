using System.Linq;
using RaftCore.Models;
using TinyFp;
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

        private Unit ReplicateLogToFollower(Descriptor descriptor, int follower)
            => descriptor.SentLength[follower]
                .Map(length => (Length: length, PrevLogTerm: length > 0 ? descriptor.Log[length - 1].Term : 0))
                .Tee(_ => _cluster.SendMessage(follower, new LogRequestMessage
                {
                    Type = MessageType.LogRequest,
                    LeaderId = Configuration.Id,
                    CurrentTerm = descriptor.CurrentTerm,
                    StartLength = _.Length,
                    PrevTerm = _.PrevLogTerm,
                    CommitLength = descriptor.CommitLenght,
                    Entries = descriptor.Log.TakeLast(descriptor.Log.Length - descriptor.SentLength[follower] - 1).ToArray()
                }))
                .Map(_ => Unit.Default);

        private Descriptor ReplicateLogToFollowers(Descriptor descriptor)
            => _cluster
                .Nodes
                .Filter(_ => _.Id != Configuration.Id)
                .ForEach(follower => ReplicateLogToFollower(descriptor, follower.Id))
                .Map(_ => descriptor);
    }
}

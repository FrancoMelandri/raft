using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.LogResponseChecks;
using static RaftCore.Node.Checks;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogResponse(LogResponseMessage message)
            => IsTermGreater(message, _descriptor)
                .Match(descriptor => HandleReceivedLogResponseKo(message, descriptor), 
                       _ => HandleReceivedLogResponseOk(message, _descriptor));

        private Descriptor HandleReceivedLogResponseKo(LogResponseMessage message, Descriptor descriptor)
            => new Descriptor 
            {
                CurrentTerm = message.Term,
                VotedFor = INIT_VOTED_FOR,
                Log = descriptor.Log,
                CommitLenght = descriptor.CommitLenght,
                CurrentRole = States.Follower,
                CurrentLeader = descriptor.CurrentLeader,
                VotesReceived = descriptor.VotesReceived,
                SentLength = descriptor.SentLength,
                AckedLength = descriptor.AckedLength
            }
            .Tee(desc => _descriptor = desc);

        private Descriptor HandleReceivedLogResponseOk(LogResponseMessage message, Descriptor descriptor)
            => IsTermEqual(message, descriptor)
                .Bind(desc => IsLeader(desc))
                .Match(desc => UpdateDescriptorAndCommitEntries(message, desc), 
                       _ => descriptor);

        private Descriptor UpdateDescriptorAndCommitEntries(LogResponseMessage message, Descriptor descriptor)
            => descriptor
                .Tee(desc => desc.SentLength[message.NodeId] = message.Ack)
                .Tee(desc => desc.AckedLength[message.NodeId] = message.Ack)
                .Map(desc => new Descriptor
                {
                    CurrentTerm = desc.CurrentTerm,
                    VotedFor = desc.VotedFor,
                    Log = desc.Log,
                    CommitLenght = desc.CommitLenght,
                    CurrentRole = desc.CurrentRole,
                    CurrentLeader = desc.CurrentLeader,
                    VotesReceived = desc.VotesReceived,
                    SentLength = desc.SentLength,
                    AckedLength = desc.AckedLength
                })
                .Tee(desc => _descriptor = desc);
    }
}

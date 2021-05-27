using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.LogReceivedChecks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogRequest(LogRequestMessage message)
            => IsTermGreater(message, _descriptor)
                .Map(descriptor => UpdateDescriptorDueTerm(message, descriptor))
                .Bind(descriptor => IsLengthOk(message, descriptor))
                .Bind(descriptor => IsTermOk(message, descriptor))
                .Bind(descriptor => IsCurrentTermOk(message, descriptor))
                .Match(_ => _, _ => _descriptor);

        private Descriptor UpdateDescriptorDueTerm(LogRequestMessage message, Descriptor descriptor)
            => new Descriptor
            {
                CurrentTerm = message.Term,
                VotedFor = INIT_VOTED_FOR,
                Log = descriptor.Log,
                CommitLenght = descriptor.CommitLenght,
                CurrentRole = descriptor.CurrentRole,
                CurrentLeader = descriptor.CurrentLeader,
                VotesReceived = descriptor.VotesReceived,
                SentLength = descriptor.SentLength,
                AckedLength = descriptor.AckedLength
            }
            .Tee(descriptor => _descriptor = descriptor);
    }
}

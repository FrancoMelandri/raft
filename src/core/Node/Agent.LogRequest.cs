using RaftCore.Models;
using static RaftCore.Node.Checks;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogRequest(LogRequestMessage message)
            => IsTermGreater(message, _descriptor)
                .Map(_ => UpdateDescriptorDueTerm(message))
                .Match(_ => _, _ => _descriptor);

        private Descriptor UpdateDescriptorDueTerm(LogRequestMessage message)
            => new Descriptor
            {
                CurrentTerm = message.Term,
                VotedFor = INIT_VOTED_FOR,
                Log = _descriptor.Log,
                CommitLenght = _descriptor.CommitLenght,
                CurrentRole = _descriptor.CurrentRole,
                CurrentLeader = _descriptor.CurrentLeader,
                VotesReceived = _descriptor.VotesReceived,
                SentLength = _descriptor.SentLength,
                AckedLength = _descriptor.AckedLength
            };
    }
}

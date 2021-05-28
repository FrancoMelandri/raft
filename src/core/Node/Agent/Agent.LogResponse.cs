using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.LogResponseChecks;
using static RaftCore.Constants.NodeConstants;


namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogResponse(LogResponseMessage message)
            => IsTermGreater(message, _descriptor)
                .Match(descriptor => HandleReceivedLogResponseKo(message, descriptor), 
                       _ => _descriptor);

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
    }
}

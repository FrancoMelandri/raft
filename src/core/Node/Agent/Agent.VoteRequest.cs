using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.MessageConstants;
using static RaftCore.Node.VoteResponseChecks;
using static RaftCore.Node.VoteRequesChecks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReceivedVoteRequest(VoteRequestMessage message)
            => ValidateLog(_descriptor, message)
                .Bind(_ => ValidateTerm(_descriptor, message))
                .Match(_ => ReceivedVoteRequestGrantResponse(message),
                       _ => ReceivedVoteRequestDontGrantResponse(message))
                .Map(_ => _descriptor);

        private Unit ReceivedVoteRequestGrantResponse(VoteRequestMessage message)
            => new Status
                {
                    CurrentTerm = message.CurrentTerm,
                    VotedFor = message.NodeId,
                    Log = _descriptor.Log,
                    CommitLenght = _descriptor.CommitLenght,
                    CurrentRole = States.Follower,
                    CurrentLeader = _descriptor.CurrentLeader,
                    VotesReceived = _descriptor.VotesReceived,
                    SentLength = _descriptor.SentLength,
                    AckedLength = _descriptor.AckedLength
                }
                .Tee(descriptor => _descriptor = descriptor)
                .Map(descriptor => _cluster.SendMessage(message.NodeId, new VoteResponseMessage
                {
                    Type = MessageType.VoteResponse,
                    NodeId = _configuration.Id,
                    CurrentTerm = _descriptor.CurrentTerm,
                    Granted = GRANT
                }));

        private Unit ReceivedVoteRequestDontGrantResponse(VoteRequestMessage message)
            => _cluster.SendMessage(message.NodeId, new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = _configuration.Id,
                CurrentTerm = _descriptor.CurrentTerm,
                Granted = NO_GRANT
            });
    }
}

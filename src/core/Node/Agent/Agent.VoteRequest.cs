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
            => ValidateLog(message, _status)
                .Bind(_ => ValidateTerm(message, _status))
                .Match(_ => ReceivedVoteRequestGrantResponse(message),
                       _ => ReceivedVoteRequestDontGrantResponse(message))
                .Map(_ => _status);

        private Unit ReceivedVoteRequestGrantResponse(VoteRequestMessage message)
            => new Status
                {
                    CurrentTerm = message.CurrentTerm,
                    VotedFor = message.NodeId,
                    Log = _status.Log,
                    CommitLenght = _status.CommitLenght,
                    CurrentRole = States.Follower,
                    CurrentLeader = _status.CurrentLeader,
                    VotesReceived = _status.VotesReceived,
                    SentLength = _status.SentLength,
                    AckedLength = _status.AckedLength
                }
                .Tee(s => _status = s)
                .Map(s => _cluster.SendMessage(message.NodeId, new VoteResponseMessage
                {
                    Type = MessageType.VoteResponse,
                    NodeId = _nodeConfiguration.Id,
                    CurrentTerm = _status.CurrentTerm,
                    Granted = GRANT
                }));

        private Unit ReceivedVoteRequestDontGrantResponse(VoteRequestMessage message)
            => _cluster.SendMessage(message.NodeId, new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = _nodeConfiguration.Id,
                CurrentTerm = _status.CurrentTerm,
                Granted = NO_GRANT
            });
    }
}

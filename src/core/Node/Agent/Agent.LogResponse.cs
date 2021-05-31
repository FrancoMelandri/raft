using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.LogResponseChecks;
using static RaftCore.Node.Checks;
using static RaftCore.Constants.NodeConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReceivedLogResponse(LogResponseMessage message)
            => IsTermGreater(message, _status)
                .Match(s => HandleReceivedLogResponseKo(message, s), 
                       _ => HandleReceivedLogResponseOk(message, _status));

        private Status HandleReceivedLogResponseKo(LogResponseMessage message, Status status)
            => new Status 
            {
                CurrentTerm = message.Term,
                VotedFor = INIT_VOTED_FOR,
                Log = status.Log,
                CommitLenght = status.CommitLenght,
                CurrentRole = States.Follower,
                CurrentLeader = status.CurrentLeader,
                VotesReceived = status.VotesReceived,
                SentLength = status.SentLength,
                AckedLength = status.AckedLength
            }
            .Tee(s => _status = s);

        private Status HandleReceivedLogResponseOk(LogResponseMessage message, Status status)
            => IsTermEqual(message, status)
                .Bind(s => IsLeader(s))
                .Match(s => IsSuccessLogReponse(message, status)
                                .Match(_ => HandleSuccessLogResponse(message, s), 
                                       _ => HandleUnSuccessLogResponse(message, s)),
                       _ => status);

        private Status HandleSuccessLogResponse(LogResponseMessage message, Status status)
            => status
                .Tee(s => s.SentLength[message.NodeId] = message.Ack)
                .Tee(s => s.AckedLength[message.NodeId] = message.Ack)
                .Map(s => new Status
                {
                    CurrentTerm = s.CurrentTerm,
                    VotedFor = s.VotedFor,
                    Log = s.Log,
                    CommitLenght = s.CommitLenght,
                    CurrentRole = s.CurrentRole,
                    CurrentLeader = s.CurrentLeader,
                    VotesReceived = s.VotesReceived,
                    SentLength = s.SentLength,
                    AckedLength = s.AckedLength
                })
                .Tee(s => _status = s)
                .Tee(s => CommitLogEntries(s));

        private Status HandleUnSuccessLogResponse(LogResponseMessage message, Status status)
            => IsSentLengthGreaterThanZero(message, status)
                .Match(_ => _
                            .Tee(s => s.SentLength[message.NodeId] = s.SentLength[message.NodeId] - 1)
                            .Map(s => new Status
                            {
                                CurrentTerm = s.CurrentTerm,
                                VotedFor = s.VotedFor,
                                Log = s.Log,
                                CommitLenght = s.CommitLenght,
                                CurrentRole = s.CurrentRole,
                                CurrentLeader = s.CurrentLeader,
                                VotesReceived = s.VotesReceived,
                                SentLength = s.SentLength,
                                AckedLength = s.AckedLength
                            })
                            .Tee(s => _status = s)
                            .Tee(s => ReplicateLog(s, message.NodeId)), 
                       _ => status);

    }
}

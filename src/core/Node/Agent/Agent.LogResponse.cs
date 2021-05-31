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
            => IsTermGreater(message, _descriptor)
                .Match(descriptor => HandleReceivedLogResponseKo(message, descriptor), 
                       _ => HandleReceivedLogResponseOk(message, _descriptor));

        private Status HandleReceivedLogResponseKo(LogResponseMessage message, Status descriptor)
            => new Status 
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

        private Status HandleReceivedLogResponseOk(LogResponseMessage message, Status descriptor)
            => IsTermEqual(message, descriptor)
                .Bind(desc => IsLeader(desc))
                .Match(desc => IsSuccessLogReponse(message, descriptor)
                                .Match(_ => HandleSuccessLogResponse(message, desc), 
                                       _ => HandleUnSuccessLogResponse(message, desc)),
                       _ => descriptor);

        private Status HandleSuccessLogResponse(LogResponseMessage message, Status descriptor)
            => descriptor
                .Tee(desc => desc.SentLength[message.NodeId] = message.Ack)
                .Tee(desc => desc.AckedLength[message.NodeId] = message.Ack)
                .Map(desc => new Status
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
                .Tee(desc => _descriptor = desc)
                .Tee(desc => CommitLogEntries(desc));

        private Status HandleUnSuccessLogResponse(LogResponseMessage message, Status descriptor)
            => IsSentLengthGreaterThanZero(message, descriptor)
                .Match(_ => _
                            .Tee(desc => desc.SentLength[message.NodeId] = desc.SentLength[message.NodeId] - 1)
                            .Map(desc => new Status
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
                            .Tee(desc => _descriptor = desc)
                            .Tee(desc => ReplicateLog(desc, message.NodeId)), 
                       _ => descriptor);

    }
}

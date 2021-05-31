using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.LogRequestChecks;
using static RaftCore.Constants.MessageConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReceivedLogRequest(LogRequestMessage message)
            => IsTermGreater(message, _status)
                .Map(s => UpdateDescriptorDueTerm(message, s))
                .Bind(s => IsLengthOk(message, s))
                .Bind(s => IsTermOk(message, s))
                .Bind(s => IsCurrentTermOk(message, s))
                .Match(_ => HandleReceivedLogRequestOk(message, _),
                       _ => HandleReceivedLogRequestKo(message, _status));

        private Status UpdateDescriptorDueTerm(LogRequestMessage message, Status status)
            => new Status
            {
                CurrentTerm = message.Term,
                VotedFor = INIT_VOTED_FOR,
                Log = status.Log,
                CommitLenght = status.CommitLenght,
                CurrentRole = status.CurrentRole,
                CurrentLeader = status.CurrentLeader,
                VotesReceived = status.VotesReceived,
                SentLength = status.SentLength,
                AckedLength = status.AckedLength
            }
            .Tee(s => _status = s);

        private Status HandleReceivedLogRequestOk(LogRequestMessage message, Status status)
            => status.Map(desc => new Status
            {
                CurrentTerm = desc.CurrentTerm,
                VotedFor = desc.VotedFor,
                Log = desc.Log,
                CommitLenght = desc.CommitLenght,
                CurrentRole = States.Follower,
                CurrentLeader = message.LeaderId,
                VotesReceived = desc.VotesReceived,
                SentLength = desc.SentLength,
                AckedLength = desc.AckedLength
            })
            .Tee(s => _status = s)
            .Tee(s => AppendEntries(message, s))
            .Tee(s => _cluster.SendMessage(message.LeaderId,
                                              new LogResponseMessage
                                              {
                                                  Type = MessageType.LogResponse,
                                                  NodeId = _configuration.Id,
                                                  Term = s.CurrentTerm,
                                                  Ack = message.LogLength + message.Entries.Length,
                                                  Success = OK_ACK
                                              }));

        private Status HandleReceivedLogRequestKo(LogRequestMessage message, Status status)
            => status.Tee(_ => _cluster.SendMessage(message.LeaderId,
                                                        new LogResponseMessage
                                                        {
                                                            Type = MessageType.LogResponse,
                                                            NodeId = _configuration.Id,
                                                            Term = _.CurrentTerm,
                                                            Ack = KO_LENGTH,
                                                            Success = KO_ACK
                                                        }));
    }
}

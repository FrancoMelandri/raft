using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.LogReceivedChecks;
using static RaftCore.Constants.MessageConstants;
using System.Linq;

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
                .Match(_ => HandleReceivedLogRequestOk(message, _), 
                       _ => HandleReceivedLogRequestKo(message, _descriptor));

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

        private Descriptor HandleReceivedLogRequestOk(LogRequestMessage message, Descriptor descriptor)
            => descriptor.Tee(_ => new Descriptor
            {
                CurrentTerm = descriptor.CurrentTerm,
                VotedFor = descriptor.VotedFor,
                Log = descriptor.Log,
                CommitLenght = descriptor.CommitLenght,
                CurrentRole = States.Follower,
                CurrentLeader = message.LeaderId,
                VotesReceived = descriptor.VotesReceived,
                SentLength = descriptor.SentLength,
                AckedLength = descriptor.AckedLength
            })
            .Tee(descriptor => _descriptor = descriptor)
            .Tee(descriptor => AppendEnries(message, descriptor))
            .Tee(descriptor => _cluster.SendMessage(message.LeaderId,
                                                        new LogResponseMessage
                                                        {
                                                            Type = MessageType.LogResponse,
                                                            NodeId = _configuration.Id,
                                                            CurrentTerm = descriptor.CurrentTerm,
                                                            Length = message.LogLength + message.Entries.Length,
                                                            Ack = OK_ACK
                                                        }));

        private Descriptor HandleReceivedLogRequestKo(LogRequestMessage message, Descriptor descriptor)
            => descriptor.Tee(_ => _cluster.SendMessage(message.LeaderId, 
                                                        new LogResponseMessage 
                                                        {
                                                            Type = MessageType.LogResponse,
                                                            NodeId = _configuration.Id,
                                                            CurrentTerm = descriptor.CurrentTerm,
                                                            Length = KO_LENGTH,
                                                            Ack = KO_ACK
                                                        }));

        private Descriptor AppendEnries(LogRequestMessage message, Descriptor descriptor)
            => TruncateLog(message, descriptor);

        private Descriptor TruncateLog(LogRequestMessage message, Descriptor descriptor)
            => IsEntriesLogLengthOk(message, descriptor)
                .Bind(_ => IsEntriesTermhNotOk(message, _))
                .Match(_ => _.Tee(descriptor => new Descriptor
                            {
                                CurrentTerm = descriptor.CurrentTerm,
                                VotedFor = descriptor.VotedFor,
                                Log = descriptor.Log.Take(message.LogLength).ToArray(),
                                CommitLenght = descriptor.CommitLenght,
                                CurrentRole = descriptor.CurrentRole,
                                CurrentLeader = descriptor.CurrentLeader,
                                VotesReceived = descriptor.VotesReceived,
                                SentLength = descriptor.SentLength,
                                AckedLength = descriptor.AckedLength
                            })
                            .Tee(descriptor => _descriptor = descriptor), 
                       _ => _descriptor);
    }
}

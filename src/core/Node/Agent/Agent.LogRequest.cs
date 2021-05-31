﻿using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.LogRequestChecks;
using static RaftCore.Constants.MessageConstants;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReceivedLogRequest(LogRequestMessage message)
            => IsTermGreater(message, _descriptor)
                .Map(descriptor => UpdateDescriptorDueTerm(message, descriptor))
                .Bind(descriptor => IsLengthOk(message, descriptor))
                .Bind(descriptor => IsTermOk(message, descriptor))
                .Bind(descriptor => IsCurrentTermOk(message, descriptor))
                .Match(_ => HandleReceivedLogRequestOk(message, _), 
                       _ => HandleReceivedLogRequestKo(message, _descriptor));

        private Status UpdateDescriptorDueTerm(LogRequestMessage message, Status descriptor)
            => new Status
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
            .Tee(desc => _descriptor = desc);

        private Status HandleReceivedLogRequestOk(LogRequestMessage message, Status descriptor)
            => descriptor.Map(desc => new Status
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
            .Tee(desc => _descriptor = desc)
            .Tee(desc => AppendEntries(message, desc))
            .Tee(desc => _cluster.SendMessage(message.LeaderId,
                                              new LogResponseMessage
                                              {
                                                  Type = MessageType.LogResponse,
                                                  NodeId = _configuration.Id,
                                                  Term = desc.CurrentTerm,
                                                  Ack = message.LogLength + message.Entries.Length,
                                                  Success = OK_ACK
                                              }));

        private Status HandleReceivedLogRequestKo(LogRequestMessage message, Status descriptor)
            => descriptor.Tee(_ => _cluster.SendMessage(message.LeaderId,
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

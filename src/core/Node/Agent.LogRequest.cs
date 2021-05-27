﻿using RaftCore.Models;
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
            .Tee(desc => _descriptor = desc);

        private Descriptor HandleReceivedLogRequestOk(LogRequestMessage message, Descriptor descriptor)
            => descriptor.Map(desc => new Descriptor
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
            .Tee(desc => AppendEnries(message, desc))
            .Tee(desc => _cluster.SendMessage(message.LeaderId,
                                                        new LogResponseMessage
                                                        {
                                                            Type = MessageType.LogResponse,
                                                            NodeId = _configuration.Id,
                                                            CurrentTerm = desc.CurrentTerm,
                                                            Length = message.LogLength + message.Entries.Length,
                                                            Ack = OK_ACK
                                                        }));

        private Descriptor HandleReceivedLogRequestKo(LogRequestMessage message, Descriptor descriptor)
            => descriptor.Tee(_ => _cluster.SendMessage(message.LeaderId, 
                                                        new LogResponseMessage 
                                                        {
                                                            Type = MessageType.LogResponse,
                                                            NodeId = _configuration.Id,
                                                            CurrentTerm = _.CurrentTerm,
                                                            Length = KO_LENGTH,
                                                            Ack = KO_ACK
                                                        }));

        private Descriptor AppendEnries(LogRequestMessage message, Descriptor descriptor)
            => TruncateLog(message, descriptor)
                .Map(desc => AppendNewEnries(message, desc))
                .Map(desc => NotifyApplication(message, desc));

        private Descriptor TruncateLog(LogRequestMessage message, Descriptor descriptor)
            => IsEntriesLogLengthOk(message, descriptor)
                .Bind(_ => IsEntriesTermhNotOk(message, _))
                .Match(_ => _.Map(desc => new Descriptor
                            {
                                CurrentTerm = desc.CurrentTerm,
                                VotedFor = desc.VotedFor,
                                Log = desc.Log.Take(message.LogLength).ToArray(),
                                CommitLenght = desc.CommitLenght,
                                CurrentRole = desc.CurrentRole,
                                CurrentLeader = desc.CurrentLeader,
                                VotesReceived = desc.VotesReceived,
                                SentLength = desc.SentLength,
                                AckedLength = desc.AckedLength
                            })
                            .Tee(desc => _descriptor = desc), 
                       _ => descriptor);

        private Descriptor AppendNewEnries(LogRequestMessage message, Descriptor descriptor)
            => AreThereEntriesToAdd(message, descriptor)
                .Match(_ => _.Map(desc => new Descriptor
                            {
                                CurrentTerm = desc.CurrentTerm,
                                VotedFor = desc.VotedFor,
                                Log = desc.Log.Concat(message.Entries).ToArray(),
                                CommitLenght = desc.CommitLenght,
                                CurrentRole = desc.CurrentRole,
                                CurrentLeader = desc.CurrentLeader,
                                VotesReceived = desc.VotesReceived,
                                SentLength = desc.SentLength,
                                AckedLength = desc.AckedLength
                            })
                            .Tee(desc => _descriptor = desc),
                       _ => descriptor);

        private Descriptor NotifyApplication(LogRequestMessage message, Descriptor descriptor)
            => AreThereUncommitedMessages(message, descriptor)
                .Match(_ => _.Tee(desc => desc.Log
                                            .TakeLast(message.CommitLength - descriptor.CommitLenght)
                                            .ForEach(log => _application.NotifyMessage(log.Message)))
                             .Map(desc => new Descriptor
                                    {
                                        CurrentTerm = desc.CurrentTerm,
                                        VotedFor = desc.VotedFor,
                                        Log = desc.Log,
                                        CommitLenght = message.CommitLength,
                                        CurrentRole = desc.CurrentRole,
                                        CurrentLeader = desc.CurrentLeader,
                                        VotesReceived = desc.VotesReceived,
                                        SentLength = desc.SentLength,
                                        AckedLength = desc.AckedLength
                                    })
                              .Tee(desc => _descriptor = desc),
                       _ => descriptor);
    }
}

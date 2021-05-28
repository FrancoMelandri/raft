using System.Linq;
using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.LogRequestChecks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        private Descriptor AppendEntries(LogRequestMessage message, Descriptor descriptor)
            => TruncateLog(message, descriptor)
                .Map(desc => AppendNewEntries(message, desc))
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

        private Descriptor AppendNewEntries(LogRequestMessage message, Descriptor descriptor)
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

        private Descriptor ReplicateLog(Descriptor descriptor, int follower)
            => descriptor.SentLength[follower]
                .Map(length => (Length: length, PrevLogTerm: length > 0 ? descriptor.Log[length - 1].Term : 0))
                .Tee(_ => _cluster.SendMessage(follower, new LogRequestMessage
                {
                    Type = MessageType.LogRequest,
                    LeaderId = _configuration.Id,
                    Term = descriptor.CurrentTerm,
                    LogLength = _.Length,
                    LogTerm = _.PrevLogTerm,
                    CommitLength = descriptor.CommitLenght,
                    Entries = descriptor.Log.TakeLast(descriptor.Log.Length - descriptor.SentLength[follower] - 1).ToArray()
                }))
                .Map(_ => descriptor);

        private Descriptor CommitLogEntries(Descriptor descriptor)
            => descriptor;
    }
}

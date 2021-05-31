using System.Linq;
using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.LogRequestChecks;
using static RaftCore.Node.Checks;
using static RaftCore.Utils;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status AppendEntries(LogRequestMessage message, Status descriptor)
            => TruncateLog(message, descriptor)
                .Map(desc => AppendNewEntries(message, desc))
                .Map(desc => NotifyApplication(message, desc));

        private Status TruncateLog(LogRequestMessage message, Status descriptor)
            => IsEntriesLogLengthOk(message, descriptor)
                .Bind(_ => IsEntriesTermhNotOk(message, _))
                .Match(_ => _.Map(desc => new Status
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

        private Status AppendNewEntries(LogRequestMessage message, Status descriptor)
            => AreThereEntriesToAdd(message, descriptor)
                .Match(_ => _.Map(desc => new Status
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

        private Status NotifyApplication(LogRequestMessage message, Status descriptor)
            => AreThereUncommitedMessages(message, descriptor)
                .Match(_ => _.Tee(desc => desc.Log
                                            .TakeLast(message.CommitLength - descriptor.CommitLenght)
                                            .ForEach(log => _application.NotifyMessage(log.Message)))
                             .Map(desc => new Status
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

        public Status ReplicateLog(Status descriptor, int follower)
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

        public Status CommitLogEntries(Status descriptor)
            => Enumerable
                .Range(1, descriptor.Log.Length)
                .Filter(index => HasQuorum(descriptor, index))
                .ToArray()
                .ToOption(_ => _.Length == 0)
                .Map(_ => _.Max())
                .Match(
                    ready => IsApplicationToBeNotified(descriptor, ready)
                                .Match(desc => NotifyToApplication(desc, ready),
                                _ => descriptor),
                    () => descriptor
                );

        private bool HasQuorum(Status descriptor, int index)
            => descriptor
                .AckedLength
                .Filter(ack => ack.Value >= index)
                .Count() >= GetQuorum(_cluster);

        private Status NotifyToApplication(Status descriptor, int ready)
            => Enumerable
                .Range(descriptor.CommitLenght, ready - descriptor.CommitLenght)
                .ForEach(_ => _application.NotifyMessage(descriptor.Log[_].Message))
                .Map(_ => descriptor)
                .Map(desc => new Status
                {
                    CurrentTerm = desc.CurrentTerm,
                    VotedFor = desc.VotedFor,
                    Log = desc.Log,
                    CommitLenght = ready,
                    CurrentRole = desc.CurrentRole,
                    CurrentLeader = desc.CurrentLeader,
                    VotesReceived = desc.VotesReceived,
                    SentLength = desc.SentLength,
                    AckedLength = desc.AckedLength
                })
                .Tee(desc => _descriptor = desc);                
    }
}
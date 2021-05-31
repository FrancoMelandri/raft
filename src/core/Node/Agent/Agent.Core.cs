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
        public Status AppendEntries(LogRequestMessage message, Status status)
            => TruncateLog(message, status)
                .Map(s => AppendNewEntries(message, s))
                .Map(s => NotifyApplication(message, s));

        private Status TruncateLog(LogRequestMessage message, Status status)
            => IsEntriesLogLengthOk(message, status)
                .Bind(_ => IsEntriesTermhNotOk(message, _))
                .Match(_ => _.Map(s => new Status
                            {
                                CurrentTerm = s.CurrentTerm,
                                VotedFor = s.VotedFor,
                                Log = s.Log.Take(message.LogLength).ToArray(),
                                CommitLenght = s.CommitLenght,
                                CurrentRole = s.CurrentRole,
                                CurrentLeader = s.CurrentLeader,
                                VotesReceived = s.VotesReceived,
                                SentLength = s.SentLength,
                                AckedLength = s.AckedLength
                            })
                            .Tee(s => _status = s),
                       _ => status);

        private Status AppendNewEntries(LogRequestMessage message, Status status)
            => AreThereEntriesToAdd(message, status)
                .Match(_ => _.Map(s => new Status
                            {
                                CurrentTerm = s.CurrentTerm,
                                VotedFor = s.VotedFor,
                                Log = s.Log.Concat(message.Entries).ToArray(),
                                CommitLenght = s.CommitLenght,
                                CurrentRole = s.CurrentRole,
                                CurrentLeader = s.CurrentLeader,
                                VotesReceived = s.VotesReceived,
                                SentLength = s.SentLength,
                                AckedLength = s.AckedLength
                            })
                            .Tee(s => _status = s),
                       _ => status);

        private Status NotifyApplication(LogRequestMessage message, Status status)
            => AreThereUncommitedMessages(message, status)
                .Match(_ => _.Tee(s => s.Log
                                        .TakeLast(message.CommitLength - status.CommitLenght)
                                        .ForEach(log => _application.NotifyMessage(log.Message)))
                             .Map(s => new Status
                             {
                                 CurrentTerm = s.CurrentTerm,
                                 VotedFor = s.VotedFor,
                                 Log = s.Log,
                                 CommitLenght = message.CommitLength,
                                 CurrentRole = s.CurrentRole,
                                 CurrentLeader = s.CurrentLeader,
                                 VotesReceived = s.VotesReceived,
                                 SentLength = s.SentLength,
                                 AckedLength = s.AckedLength
                             })
                              .Tee(s => _status = s),
                       _ => status);

        public Status ReplicateLog(Status status, int follower)
            => status.SentLength[follower]
                .Map(length => (Length: length, PrevLogTerm: length > 0 ? status.Log[length - 1].Term : 0))
                .Tee(_ => _cluster.SendMessage(follower, new LogRequestMessage
                {
                    Type = MessageType.LogRequest,
                    LeaderId = _configuration.Id,
                    Term = status.CurrentTerm,
                    LogLength = _.Length,
                    LogTerm = _.PrevLogTerm,
                    CommitLength = status.CommitLenght,
                    Entries = status.Log.TakeLast(status.Log.Length - status.SentLength[follower] - 1).ToArray()
                }))
                .Map(_ => status);

        public Status CommitLogEntries(Status status)
            => Enumerable
                .Range(1, status.Log.Length)
                .Filter(index => HasQuorum(status, index))
                .ToArray()
                .ToOption(_ => _.Length == 0)
                .Map(_ => _.Max())
                .Match(
                    ready => IsApplicationToBeNotified(status, ready)
                                .Match(desc => NotifyToApplication(desc, ready),
                                _ => status),
                    () => status
                );

        private bool HasQuorum(Status status, int index)
            => status
                .AckedLength
                .Filter(ack => ack.Value >= index)
                .Count() >= GetQuorum(_cluster);

        private Status NotifyToApplication(Status status, int ready)
            => Enumerable
                .Range(status.CommitLenght, ready - status.CommitLenght)
                .ForEach(_ => _application.NotifyMessage(status.Log[_].Message))
                .Map(_ => status)
                .Map(s => new Status
                {
                    CurrentTerm = s.CurrentTerm,
                    VotedFor = s.VotedFor,
                    Log = s.Log,
                    CommitLenght = ready,
                    CurrentRole = s.CurrentRole,
                    CurrentLeader = s.CurrentLeader,
                    VotesReceived = s.VotesReceived,
                    SentLength = s.SentLength,
                    AckedLength = s.AckedLength
                })
                .Tee(s => _status = s);
    }
}
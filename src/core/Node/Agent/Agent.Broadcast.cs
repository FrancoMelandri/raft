using System.Linq;
using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnBroadcastMessage(Message message)
            => IsLeader(_descriptor)
                .Match(_ => HandleBroadcastMessageAsLeader(message),
                       _ => _descriptor.Tee(descriptor => _cluster.SendMessage(descriptor.CurrentLeader, message)));

        private Status HandleBroadcastMessageAsLeader(Message message)
            => (Log: _descriptor.Log.Concat(new LogEntry[] { new LogEntry { Message = message, Term = _descriptor.CurrentTerm } }).ToArray(),
                AckedLength: _descriptor.AckedLength.Tee(_ => _[_configuration.Id] = _descriptor.Log.Length + 1))
                .Map(_ => new Status
                {
                    CurrentTerm = _descriptor.CurrentTerm,
                    VotedFor = _descriptor.VotedFor,
                    Log = _.Log,
                    CommitLenght = _descriptor.CommitLenght,
                    CurrentRole = _descriptor.CurrentRole,
                    CurrentLeader = _descriptor.CurrentLeader,
                    VotesReceived = _descriptor.VotesReceived,
                    SentLength = _descriptor.SentLength,
                    AckedLength = _.AckedLength
                })
                .Tee(_ => ReplicateLogToFollowers(_));
    }    
}

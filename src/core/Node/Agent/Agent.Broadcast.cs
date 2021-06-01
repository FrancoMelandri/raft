using System.Linq;
using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Node.Checks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnBroadcastMessage(Message message)
            => IsLeader(_status)
                .Match(_ => HandleBroadcastMessageAsLeader(message),
                       _ => _status.Tee(status => _cluster.SendMessage(status.CurrentLeader, message)));

        private Status HandleBroadcastMessageAsLeader(Message message)
            => (Log: _status.Log.Concat(new LogEntry[] { new LogEntry { Message = message, Term = _status.CurrentTerm } }).ToArray(),
                AckedLength: _status.AckedLength.Tee(_ => _[_localNodeConfiguration.Id] = _status.Log.Length + 1))
                .Map(_ => new Status
                {
                    CurrentTerm = _status.CurrentTerm,
                    VotedFor = _status.VotedFor,
                    Log = _.Log,
                    CommitLenght = _status.CommitLenght,
                    CurrentRole = _status.CurrentRole,
                    CurrentLeader = _status.CurrentLeader,
                    VotesReceived = _status.VotesReceived,
                    SentLength = _status.SentLength,
                    AckedLength = _.AckedLength
                })
                .Tee(_ => ReplicateLogToFollowers(_));
    }    
}

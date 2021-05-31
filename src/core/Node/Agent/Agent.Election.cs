using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Utils;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnLeaderHasFailed()
            => new Status
            {
                CurrentTerm = _status.CurrentTerm + 1,
                VotedFor = _configuration.Id,
                Log = _status.Log,
                CommitLenght = _status.CommitLenght,
                CurrentRole = States.Candidate,
                CurrentLeader = _status.CurrentLeader,
                VotesReceived = new[] { _configuration.Id },
                SentLength = _status.SentLength,
                AckedLength = _status.AckedLength
            }
            .Tee(s => _status = s)
            .Tee(s => LastEntryOrZero(s.Log)
                            .Map(lastTerm => new VoteRequestMessage
                            {
                                Type = MessageType.VoteRequest,
                                NodeId = _configuration.Id,
                                CurrentTerm = _status.CurrentTerm,
                                LogLength = _status.Log.ToOption().Map(_ => _.Length).OnNone(0),
                                LastTerm = lastTerm
                            })
                            .Tee(_ => _cluster.SendBroadcastMessage(_))
                            .Tee(_ => _election.Start()))
            .Map(_ => _status);

        public Status OnElectionTimeOut()
            => OnLeaderHasFailed();
    }   
}

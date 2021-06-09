using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static RaftCore.Constants.Logs;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnLeaderHasFailed()
            => OnStartVoteRequestPhase(LEADER_HAS_FAILED);

        public Status OnElectionTimeOut()
            => OnStartVoteRequestPhase(ELECTION_TIMEOUT);

        private Status OnStartVoteRequestPhase(string log)
            => new Status
            {
                CurrentTerm = _status.CurrentTerm + 1,
                VotedFor = _nodeConfiguration.Id,
                Log = _status.Log,
                CommitLenght = _status.CommitLenght,
                CurrentRole = States.Candidate,
                CurrentLeader = _status.CurrentLeader,
                VotesReceived = new[] { _nodeConfiguration.Id },
                SentLength = _status.SentLength,
                AckedLength = _status.AckedLength
            }
            .Tee(s => _status = s)
            .Tee(s => LastEntryTermOrZero(s.Log)
                            .Map(lastTerm => new VoteRequestMessage
                            {
                                Type = MessageType.VoteRequest,
                                NodeId = _nodeConfiguration.Id,
                                CurrentTerm = _status.CurrentTerm,
                                LogLength = _status.Log.ToOption().Map(_ => _.Length).OnNone(0),
                                LastTerm = lastTerm
                            })
                            .Tee(_ => _cluster.SendBroadcastMessage(_))
                            .Tee(_ => Election.Start()))
            .Map(_ => _status)
            .Tee(_ => _logger.Information(log));

    }
}

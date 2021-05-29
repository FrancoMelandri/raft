using RaftCore.Models;
using TinyFp.Extensions;
using static RaftCore.Utils;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnLeaderHasFailed()
            => new Descriptor
            {
                CurrentTerm = _descriptor.CurrentTerm + 1,
                VotedFor = _configuration.Id,
                Log = _descriptor.Log,
                CommitLenght = _descriptor.CommitLenght,
                CurrentRole = States.Candidate,
                CurrentLeader = _descriptor.CurrentLeader,
                VotesReceived = new[] { _configuration.Id },
                SentLength = _descriptor.SentLength,
                AckedLength = _descriptor.AckedLength
            }
            .Tee(descriptor => _descriptor = descriptor)
            .Tee(descriptor => LastEntryOrZero(descriptor.Log)
                                .Map(lastTerm => new VoteRequestMessage
                                {
                                    Type = MessageType.VoteRequest,
                                    NodeId = _configuration.Id,
                                    CurrentTerm = _descriptor.CurrentTerm,
                                    LogLength = _descriptor.Log.ToOption().Map(_ => _.Length).OnNone(0),
                                    LastTerm = lastTerm
                                })
                                .Tee(_ => _cluster.SendBroadcastMessage(_))
                                .Tee(_ => _election.Start()))
            .Map(_ => _descriptor);

        public Descriptor OnElectionTimeOut()
            => OnLeaderHasFailed();
    }   
}

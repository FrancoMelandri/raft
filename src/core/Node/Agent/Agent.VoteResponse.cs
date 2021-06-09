using System.Collections.Generic;
using System.Linq;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Constants.Logs;
using static RaftCore.Node.VoteResponseChecks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Status OnReceivedVoteResponse(VoteResponseMessage message)
            => ValidateVoteGrant(message, _status)
                .Match(_ => ReceivedVoteResponseGranted(message),
                       _ => ValidateTerm(message, _status)
                                .Match(__ => LogInformation($"{VOTE_NOT_GRANTED} {message.NodeId}")
                                                .Map(___ => ReceivedVoteResponseNoGrantedUpdateStatus(message)),
                                       __ => LogError(__).Map(___ =>  Unit.Default)))
                .Map(_ => _status);

        private Unit ReceivedVoteResponseGranted(VoteResponseMessage message)
            => new Status
                {
                    CurrentTerm = _status.CurrentTerm,
                    VotedFor = _status.VotedFor,
                    Log = _status.Log,
                    CommitLenght = _status.CommitLenght,
                    CurrentRole = _status.CurrentRole,
                    CurrentLeader = _status.CurrentLeader,
                    VotesReceived = _status.VotesReceived.Concat(new int[] { message.NodeId }).Distinct().ToArray(),
                    SentLength = _status.SentLength,
                    AckedLength = _status.AckedLength
                }
                .Map(s => ValidateVotesQuorum(s, _cluster)
                            .Match(_ => LogInformation(PROMOTED_AS_LEADER).Map(__ =>  ReceivedVoteResponseGrantedPromoteAsLeader(s)),
                                   _ => LogError(_).Map(__ => s)))
                .Tee(s => _status = s)
                .Map(_ => Unit.Default);

        private Unit ReceivedVoteResponseNoGrantedUpdateStatus(VoteResponseMessage message)
            => new Status
                {
                    CurrentTerm = message.CurrentTerm,
                    VotedFor = INIT_VOTED_FOR,
                    Log = _status.Log,
                    CommitLenght = _status.CommitLenght,
                    CurrentRole = States.Follower,
                    CurrentLeader = _status.CurrentLeader,
                    VotesReceived = _status.VotesReceived,
                    SentLength = _status.SentLength,
                    AckedLength = _status.AckedLength
                }
                .Tee(s => _status = s)
                .Map(_ => Election.Stop())
                .Map(_ => _leader.Stop());

        private Status ReceivedVoteResponseGrantedPromoteAsLeader(Status status)
            => new Status
                {
                    CurrentTerm = status.CurrentTerm,
                    VotedFor = status.VotedFor,
                    Log = status.Log,
                    CommitLenght = status.CommitLenght,
                    CurrentRole = States.Leader,
                    CurrentLeader = _nodeConfiguration.Id,
                    VotesReceived = status.VotesReceived,
                    SentLength = status.SentLength,
                    AckedLength = status.AckedLength
                }
                .Tee(_ => Election.Stop())
                .Tee(_ => _leader.Start(this))
                .Tee(ReceivedVoteResponseGrantedUpdateFollowers);

        private Status ReceivedVoteResponseGrantedUpdateFollowers(Status status)
            => _cluster.Nodes
                .Filter(_ => _.Id != _nodeConfiguration.Id)
                .Map(_ => (_.Id, status.Log.Length))
                .Fold((SentLength: new Dictionary<int, int>(), AckedLength: new Dictionary<int, int>()),
                      (a, i) => a.Tee(_ => a.SentLength.Add(i.Id, i.Length))
                                 .Tee(_ => a.AckedLength.Add(i.Id, 0)))
                .Map(_ => (Status: new Status
                                {
                                    CurrentTerm = status.CurrentTerm,
                                    VotedFor = status.VotedFor,
                                    Log = status.Log,
                                    CommitLenght = status.CommitLenght,
                                    CurrentRole = status.CurrentRole,
                                    CurrentLeader = status.CurrentLeader,
                                    VotesReceived = status.VotesReceived,
                                    SentLength = _.SentLength,
                                    AckedLength = _.AckedLength
                                },
                           Followers: _.SentLength.Keys))
                .Tee(_ => _.Followers.ForEach(follower => ReplicateLog(_.Status, follower)))
                .Map(_ => _.Status);
    }
}

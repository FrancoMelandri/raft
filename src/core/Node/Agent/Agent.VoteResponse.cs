using System.Collections.Generic;
using System.Linq;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Constants.NodeConstants;
using static RaftCore.Node.VoteResponseChecks;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedVoteResponse(VoteResponseMessage message)
            => ValidateVoteGrant(_descriptor, message)
                .Match(_ => ReceivedVoteResponseGranted(message),
                       _ => ValidateTerm(_descriptor, message)
                                .Match(_ => ReceivedVoteResponseNoGrantedUpdateDescriptor(message),
                                        _ => Unit.Default))
                .Map(_ => _descriptor);

        private Unit ReceivedVoteResponseGranted(VoteResponseMessage message)
            => new Descriptor
                {
                    CurrentTerm = _descriptor.CurrentTerm,
                    VotedFor = _descriptor.VotedFor,
                    Log = _descriptor.Log,
                    CommitLenght = _descriptor.CommitLenght,
                    CurrentRole = _descriptor.CurrentRole,
                    CurrentLeader = _descriptor.CurrentLeader,
                    VotesReceived = _descriptor.VotesReceived.Concat(new int[] { message.NodeId }).Distinct().ToArray(),
                    SentLength = _descriptor.SentLength,
                    AckedLength = _descriptor.AckedLength
                }
                .Map(descriptor => ValidateVotesQuorum(descriptor, _cluster)
                                    .Match(_ => ReceivedVoteResponseGrantedPromoteAsLeader(descriptor),
                                           _ => descriptor))
                .Tee(descriptor => _descriptor = descriptor)
                .Map(_ => Unit.Default);

        private Unit ReceivedVoteResponseNoGrantedUpdateDescriptor(VoteResponseMessage message)
            => new Descriptor
                {
                    CurrentTerm = message.CurrentTerm,
                    VotedFor = INIT_VOTED_FOR,
                    Log = _descriptor.Log,
                    CommitLenght = _descriptor.CommitLenght,
                    CurrentRole = States.Follower,
                    CurrentLeader = _descriptor.CurrentLeader,
                    VotesReceived = _descriptor.VotesReceived,
                    SentLength = _descriptor.SentLength,
                    AckedLength = _descriptor.AckedLength
                }
                .Tee(descriptor => _descriptor = descriptor)
                .Map(_ => _election.Cancel());

        private Descriptor ReceivedVoteResponseGrantedPromoteAsLeader(Descriptor descriptor)
            => new Descriptor
                {
                    CurrentTerm = descriptor.CurrentTerm,
                    VotedFor = descriptor.VotedFor,
                    Log = descriptor.Log,
                    CommitLenght = descriptor.CommitLenght,
                    CurrentRole = States.Leader,
                    CurrentLeader = _configuration.Id,
                    VotesReceived = descriptor.VotesReceived,
                    SentLength = descriptor.SentLength,
                    AckedLength = descriptor.AckedLength
                }
                .Tee(_ => _election.Cancel())
                .Tee(desc => ReceivedVoteResponseGrantedUpdateFollowers(desc));

        private Descriptor ReceivedVoteResponseGrantedUpdateFollowers(Descriptor descriptor)
            => _cluster.Nodes
                .Filter(_ => _.Id != _configuration.Id)
                .Map(_ => (_.Id, descriptor.Log.Length))
                .Fold((SentLength: new Dictionary<int, int>(), AckedLength: new Dictionary<int, int>()),
                      (a, i) => a.Tee(_ => a.SentLength.Add(i.Id, i.Length))
                                 .Tee(_ => a.AckedLength.Add(i.Id, 0)))
                .Map(_ => (Descriptor: new Descriptor
                                {
                                    CurrentTerm = descriptor.CurrentTerm,
                                    VotedFor = descriptor.VotedFor,
                                    Log = descriptor.Log,
                                    CommitLenght = descriptor.CommitLenght,
                                    CurrentRole = descriptor.CurrentRole,
                                    CurrentLeader = descriptor.CurrentLeader,
                                    VotesReceived = descriptor.VotesReceived,
                                    SentLength = _.SentLength,
                                    AckedLength = _.AckedLength
                                },
                           Followers: _.SentLength.Keys))
                .Tee(_ => _.Followers.ForEach(follower => ReplicateLog(_.Descriptor, follower)))
                .Map(_ => _.Descriptor);
    }
}

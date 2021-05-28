using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static System.Math;

namespace RaftCore.Node
{
    public static class VoteResponseChecks
    {
        public static Either<string, int> ValidateVoteGrant(Descriptor descriptor, VoteResponseMessage message)
            => descriptor.CurrentRole == States.Candidate && 
               descriptor.CurrentTerm == message.CurrentTerm &&
               message.Granted ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateTerm(Descriptor descriptor, VoteResponseMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateVotesQuorum(Descriptor descriptor, ICluster cluster)
        => descriptor.VotesReceived.ToOption(_ => _.Length, _ => _.Length == 0).OnNone(0) > (int)Floor(((decimal)cluster.Nodes.Length + 1) / 2) ?
            Either<string, int>.Right(0) :
            Either<string, int>.Left("");
    }
}

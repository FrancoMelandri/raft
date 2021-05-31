using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class VoteResponseChecks
    {
        public static Either<string, int> ValidateVoteGrant(Descriptor descriptor, VoteResponseMessage message)
            => descriptor.CurrentRole == States.Candidate && 
               descriptor.CurrentTerm == message.CurrentTerm &&
               message.Granted ?
                Right<string, int>(0) :
                Left<string, int>("");

        public static Either<string, int> ValidateTerm(Descriptor descriptor, VoteResponseMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Right<string, int>(0) :
                Left<string, int>("");

        public static Either<string, int> ValidateVotesQuorum(Descriptor descriptor, ICluster cluster)
            => descriptor.VotesReceived.ToOption(_ => _.Length, _ => _.Length == 0).OnNone(0) > GetQuorum(cluster) ?
                Right<string, int>(0) :
                Left<string, int>("");
    }
}

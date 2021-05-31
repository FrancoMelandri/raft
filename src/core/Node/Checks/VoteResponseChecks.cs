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
        public static Either<string, int> ValidateVoteGrant(Status status, VoteResponseMessage message)
            => status.CurrentRole == States.Candidate && 
               status.CurrentTerm == message.CurrentTerm &&
               message.Granted ?
                Right<string, int>(0) :
                Left<string, int>("");

        public static Either<string, int> ValidateTerm(Status status, VoteResponseMessage message)
            => message.CurrentTerm > status.CurrentTerm ?
                Right<string, int>(0) :
                Left<string, int>("");

        public static Either<string, int> ValidateVotesQuorum(Status status, ICluster cluster)
            => status.VotesReceived.ToOption(_ => _.Length, _ => _.Length == 0).OnNone(0) > GetQuorum(cluster) ?
                Right<string, int>(0) :
                Left<string, int>("");
    }
}

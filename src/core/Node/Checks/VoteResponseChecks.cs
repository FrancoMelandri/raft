using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;
using static RaftCore.Constants.Errors;

namespace RaftCore.Node
{
    public static class VoteResponseChecks
    {
        public static Either<Error, int> ValidateVoteGrant(VoteResponseMessage message, Status status)
            => status.CurrentRole == States.Candidate && 
               status.CurrentTerm == message.CurrentTerm &&
               message.Granted ?
                Right<Error, int>(0) :
                Left<Error, int>(VoteIsNotValid);

        public static Either<Error, int> ValidateTerm(VoteResponseMessage message, Status status)
            => message.CurrentTerm > status.CurrentTerm ?
                Right<Error, int>(0) :
                Left<Error, int>(TermIsNotValid);

        public static Either<Error, int> ValidateVotesQuorum(Status status, ICluster cluster)
            => status.VotesReceived.ToOption(_ => _.Length, _ => _.Length == 0).OnNone(0) > GetQuorum(cluster) ?
                Right<Error, int>(0) :
                Left<Error, int>(QuorumNotReached);
    }
}

using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;
using static RaftCore.Constants.Errors;

namespace RaftCore.Node
{
    public static class VoteRequesChecks
    {
        public static Either<Error, Status> ValidateLog(VoteRequestMessage message, Status status)
            => ValidateLogTerm(message, status)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(message, status)));

        private static Either<Error, Status> ValidateLogTerm(VoteRequestMessage message, Status status)
            => message.LastTerm > LastEntryTermOrZero(status.Log) ?
                Right<Error, Status>(status) :
                Left<Error, Status>(LastTermAndLogAreWrong);

        private static Either<Error, Status> ValidateLogLength(VoteRequestMessage message, Status status)
            => message.LastTerm == LastEntryTermOrZero(status.Log) &&
               message.LogLength >= status.Log.Length ?
                Right<Error, Status>(status) :
                Left<Error, Status>(LastTermAndLogAreWrong);

        public static Either<Error, Status> ValidateTerm(VoteRequestMessage message, Status status)
            => ValidateCurrentTerm(message, status)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(message, status)));

        private static Either<Error, Status> ValidateCurrentTerm(VoteRequestMessage message, Status status)
            => message.CurrentTerm > status.CurrentTerm ?
                Right<Error, Status>(status) :
                Left<Error, Status>(CurrentTermAndVotedFroreWrong);

        private static Either<Error, Status> ValidateVoteFor(VoteRequestMessage message, Status status)
            => message.CurrentTerm == status.CurrentTerm &&
               status.VotedFor != message.NodeId ?
                Right<Error, Status>(status) :
                Left<Error, Status>(CurrentTermAndVotedFroreWrong);
    }
}

using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class VoteRequesChecks
    {
        public static Either<string, Status> ValidateLog(VoteRequestMessage message, Status status)
            => ValidateLogTerm(message, status)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(message, status)));

        private static Either<string, Status> ValidateLogTerm(VoteRequestMessage message, Status status)
            => message.LastTerm > LastEntryOrZero(status.Log) ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        private static Either<string, Status> ValidateLogLength(VoteRequestMessage message, Status status)
            => message.LastTerm == LastEntryOrZero(status.Log) &&
               message.LogLength >= status.Log.Length ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> ValidateTerm(VoteRequestMessage message, Status status)
            => ValidateCurrentTerm(message, status)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(message, status)));

        private static Either<string, Status> ValidateCurrentTerm(VoteRequestMessage message, Status status)
            => message.CurrentTerm > status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        private static Either<string, Status> ValidateVoteFor(VoteRequestMessage message, Status status)
            => message.CurrentTerm == status.CurrentTerm &&
               status.VotedFor != message.NodeId ?
                Right<string, Status>(status) :
                Left<string, Status>("");
    }
}

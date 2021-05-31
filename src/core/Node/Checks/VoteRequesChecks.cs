using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class VoteRequesChecks
    {
        public static Either<string, Status> ValidateLog(Status status, VoteRequestMessage message)
            => ValidateLogTerm(status, message)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(status, message)));

        private static Either<string, Status> ValidateLogTerm(Status status, VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(status.Log) ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        private static Either<string, Status> ValidateLogLength(Status descriptor, VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(descriptor.Log) &&
               message.LogLength >= descriptor.Log.Length ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> ValidateTerm(Status descriptor, VoteRequestMessage message)
            => ValidateCurrentTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(descriptor, message)));

        private static Either<string, Status> ValidateCurrentTerm(Status descriptor, VoteRequestMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        private static Either<string, Status> ValidateVoteFor(Status descriptor, VoteRequestMessage message)
            => message.CurrentTerm == descriptor.CurrentTerm &&
               descriptor.VotedFor != message.NodeId ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");
    }
}

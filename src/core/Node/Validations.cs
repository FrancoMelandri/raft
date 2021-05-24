using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Node.Utils;

namespace RaftCore.Node
{
    public static class Validations
    {
        public static Either<string, int> ValidateLog(Descriptor descriptor, VoteRequestMessage message)
            => ValidateLogTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(descriptor, message)));

        private static Either<string, int> ValidateLogTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(descriptor.Log) ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private static Either<string, int> ValidateLogLength(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(descriptor.Log) &&
               message.LogLength >= descriptor.Log.Length ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateTerm(Descriptor descriptor, VoteRequestMessage message)
            => ValidateCurrentTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(descriptor, message)));

        private static Either<string, int> ValidateCurrentTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        private static Either<string, int> ValidateVoteFor(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm == descriptor.CurrentTerm &&
               descriptor.VotedFor != message.NodeId ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");
    }
}

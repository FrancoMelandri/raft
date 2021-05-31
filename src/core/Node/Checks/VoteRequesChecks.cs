using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;
using static RaftCore.Utils;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class VoteRequesChecks
    {
        public static Either<string, Descriptor> ValidateLog(Descriptor descriptor, VoteRequestMessage message)
            => ValidateLogTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateLogLength(descriptor, message)));

        private static Either<string, Descriptor> ValidateLogTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(descriptor.Log) ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        private static Either<string, Descriptor> ValidateLogLength(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(descriptor.Log) &&
               message.LogLength >= descriptor.Log.Length ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> ValidateTerm(Descriptor descriptor, VoteRequestMessage message)
            => ValidateCurrentTerm(descriptor, message)
                .Map(_ => _.Match(_ => _, _ => ValidateVoteFor(descriptor, message)));

        private static Either<string, Descriptor> ValidateCurrentTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm > descriptor.CurrentTerm ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        private static Either<string, Descriptor> ValidateVoteFor(Descriptor descriptor, VoteRequestMessage message)
            => message.CurrentTerm == descriptor.CurrentTerm &&
               descriptor.VotedFor != message.NodeId ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");
    }
}

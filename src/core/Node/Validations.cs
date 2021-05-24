using RaftCore.Models;
using TinyFp;
using static RaftCore.Node.Utils;

namespace RaftCore.Node
{
    public static class Validations
    {
        public static Either<string, int> ValidateLogTerm(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm > LastEntryOrZero(descriptor.Log) ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");

        public static Either<string, int> ValidateLogLength(Descriptor descriptor, VoteRequestMessage message)
            => message.LastTerm == LastEntryOrZero(descriptor.Log) &&
               message.LogLength >= descriptor.Log.Length ?
                Either<string, int>.Right(0) :
                Either<string, int>.Left("");
    }
}

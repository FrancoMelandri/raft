using RaftCore.Models;
using TinyFp;

namespace RaftCore.Node
{
    public static class LogResponseChecks
    {
        public static Either<string, Descriptor> IsTermGreater(LogResponseMessage message, Descriptor descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsTermEqual(LogResponseMessage message, Descriptor descriptor)
            => message.Term == descriptor.CurrentTerm ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsSentLengthGreaterThanZero(LogResponseMessage message, Descriptor descriptor)
            => descriptor.SentLength[message.NodeId] > 0 ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsSuccessLogReponse(LogResponseMessage message, Descriptor descriptor)
            => message.Success ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");
    }
}

using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogResponseChecks
    {
        public static Either<string, Descriptor> IsTermGreater(LogResponseMessage message, Descriptor descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsTermEqual(LogResponseMessage message, Descriptor descriptor)
            => message.Term == descriptor.CurrentTerm ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsSentLengthGreaterThanZero(LogResponseMessage message, Descriptor descriptor)
            => descriptor.SentLength[message.NodeId] > 0 ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsSuccessLogReponse(LogResponseMessage message, Descriptor descriptor)
            => message.Success ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");
    }
}

using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogResponseChecks
    {
        public static Either<string, Status> IsTermGreater(LogResponseMessage message, Status descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsTermEqual(LogResponseMessage message, Status descriptor)
            => message.Term == descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsSentLengthGreaterThanZero(LogResponseMessage message, Status descriptor)
            => descriptor.SentLength[message.NodeId] > 0 ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsSuccessLogReponse(LogResponseMessage message, Status descriptor)
            => message.Success ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");
    }
}

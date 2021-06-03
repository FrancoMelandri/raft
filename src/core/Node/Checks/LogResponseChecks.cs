using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogResponseChecks
    {
        public static Either<string, Status> IsTermGreater(LogResponseMessage message, Status status)
            => message.Term > status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsTermEqual(LogResponseMessage message, Status status)
            => message.Term == status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsSentLengthGreaterThanZero(LogResponseMessage message, Status status)
            => status.SentLength.ContainsKey(message.NodeId) && 
               status.SentLength[message.NodeId] > 0 ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsSuccessLogReponse(LogResponseMessage message, Status status)
            => message.Success ?
                Right<string, Status>(status) :
                Left<string, Status>("");
    }
}

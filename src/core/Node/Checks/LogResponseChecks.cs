using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;
using static RaftCore.Constants.Errors;

namespace RaftCore.Node
{
    public static class LogResponseChecks
    {
        public static Either<Error, Status> IsTermLessOrEqualGreater(LogResponseMessage message, Status status)
            => message.Term <= status.CurrentTerm ?
                Right<Error, Status>(status) :
                Left<Error, Status>(TermIsNotGreater);

        public static Either<Error, Status> IsTermEqual(LogResponseMessage message, Status status)
            => message.Term == status.CurrentTerm ?
                Right<Error, Status>(status) :
                Left<Error, Status>(TermNotEqual);

        public static Either<Error, Status> IsSentLengthGreaterThanZero(LogResponseMessage message, Status status)
            => status.SentLength.ContainsKey(message.NodeId) && 
               status.SentLength[message.NodeId] > 0 ?
                Right<Error, Status>(status) :
                Left<Error, Status>(SentLegthIsWrong);

        public static Either<Error, Status> IsSuccessLogReponse(LogResponseMessage message, Status status)
            => message.Success ?
                Right<Error, Status>(status) :
                Left<Error, Status>(LogNotSuccess);
    }
}

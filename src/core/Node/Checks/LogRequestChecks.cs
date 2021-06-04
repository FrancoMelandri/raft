using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;
using static RaftCore.Constants.Errors;

namespace RaftCore.Node
{
    public static class LogRequestChecks
    {
        public static Either<Error, Status> IsTermGreater(LogRequestMessage message, Status status)
            => message.Term > status.CurrentTerm ?
                Right<Error, Status>(status) :
                Left<Error, Status>(TermIsNotGreater);

        public static Either<Error, Status> IsLengthOk(LogRequestMessage message, Status status)
             => status.Log.Length >= message.LogLength && message.LogLength > 0 ?
                Right<Error, Status>(status) :
                Left<Error, Status>(LengthIsNotOk);

        public static Either<Error, Status> IsTermOk(LogRequestMessage message, Status status)
             => status.Log.Length >= message.LogLength && 
                message.LogTerm == status.Log[message.LogLength - 1].Term ?
                Right<Error, Status>(status) :
                Left<Error, Status>(TermIsNotOk);

        public static Either<Error, Status> IsCurrentTermOk(LogRequestMessage message, Status status)
             => message.Term == status.CurrentTerm ?
                Right<Error, Status>(status) :
                Left<Error, Status>(CurrentTermNotOk);

        public static Either<Error, Status> IsEntriesLogLengthOk(LogRequestMessage message, Status status)
             => message.Entries.Length > 0 && status.Log.Length > message.LogLength ?
                Right<Error, Status>(status) :
                Left<Error, Status>(EntriesLogLengthNotOk);

        public static Either<Error, Status> IsEntriesTermNotOk(LogRequestMessage message, Status status)
             => message.Entries.Length > 0 &&
                status.Log.Length > message.LogLength &&
                status.Log[message.LogLength].Term != message.Entries[0].Term ?
                Right<Error, Status>(status) :
                Left<Error, Status>(EntrieTermIsNotOk);

        public static Either<Error, Status> AreThereEntriesToAdd(LogRequestMessage message, Status status)
             => message.LogLength + message.Entries.Length > status.Log.Length ?
                Right<Error, Status>(status) :
                Left<Error, Status>(Empty);

        public static Either<Error, Status> AreThereUncommitedMessages(LogRequestMessage message, Status status)
             => message.CommitLength > status.CommitLenght ?
                Right<Error, Status>(status) :
                Left<Error, Status>(Empty);
    }
}

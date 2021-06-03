using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogRequestChecks
    {
        public static Either<string, Status> IsTermGreater(LogRequestMessage message, Status status)
            => message.Term > status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsLengthOk(LogRequestMessage message, Status status)
             => status.Log.Length >= message.LogLength && message.LogLength > 0 ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsTermOk(LogRequestMessage message, Status status)
             => status.Log.Length >= message.LogLength && message.LogTerm == status.Log[message.LogLength - 1].Term ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsCurrentTermOk(LogRequestMessage message, Status status)
             => message.Term == status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsEntriesLogLengthOk(LogRequestMessage message, Status status)
             => message.Entries.Length > 0 && status.Log.Length > message.LogLength ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> IsEntriesTermNotOk(LogRequestMessage message, Status status)
             => message.Entries.Length > 0 &&
                status.Log.Length > message.LogLength &&
                status.Log[message.LogLength].Term != message.Entries[0].Term ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> AreThereEntriesToAdd(LogRequestMessage message, Status status)
             => message.LogLength + message.Entries.Length > status.Log.Length ?
                Right<string, Status>(status) :
                Left<string, Status>("");

        public static Either<string, Status> AreThereUncommitedMessages(LogRequestMessage message, Status status)
             => message.CommitLength > status.CommitLenght ?
                Right<string, Status>(status) :
                Left<string, Status>("");
    }
}

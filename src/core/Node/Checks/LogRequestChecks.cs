using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogRequestChecks
    {
        public static Either<string, Status> IsTermGreater(LogRequestMessage message, Status descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsLengthOk(LogRequestMessage message, Status descriptor)
             => descriptor.Log.Length >= message.LogLength && message.LogLength > 0 ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsTermOk(LogRequestMessage message, Status descriptor)
             => message.LogTerm == descriptor.Log[message.LogLength - 1].Term ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsCurrentTermOk(LogRequestMessage message, Status descriptor)
             => message.Term == descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsEntriesLogLengthOk(LogRequestMessage message, Status descriptor)
             => message.Entries.Length > 0 && descriptor.Log.Length > message.LogLength ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> IsEntriesTermhNotOk(LogRequestMessage message, Status descriptor)
             => descriptor.Log[message.LogLength].Term != message.Entries[0].Term ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> AreThereEntriesToAdd(LogRequestMessage message, Status descriptor)
             => message.LogLength + message.Entries.Length > descriptor.Log.Length ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");

        public static Either<string, Status> AreThereUncommitedMessages(LogRequestMessage message, Status descriptor)
             => message.CommitLength > descriptor.CommitLenght ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");
    }
}

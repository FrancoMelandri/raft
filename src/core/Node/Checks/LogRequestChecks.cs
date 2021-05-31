using RaftCore.Models;
using TinyFp;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class LogRequestChecks
    {
        public static Either<string, Descriptor> IsTermGreater(LogRequestMessage message, Descriptor descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsLengthOk(LogRequestMessage message, Descriptor descriptor)
             => descriptor.Log.Length >= message.LogLength && message.LogLength > 0 ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsTermOk(LogRequestMessage message, Descriptor descriptor)
             => message.LogTerm == descriptor.Log[message.LogLength - 1].Term ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsCurrentTermOk(LogRequestMessage message, Descriptor descriptor)
             => message.Term == descriptor.CurrentTerm ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsEntriesLogLengthOk(LogRequestMessage message, Descriptor descriptor)
             => message.Entries.Length > 0 && descriptor.Log.Length > message.LogLength ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> IsEntriesTermhNotOk(LogRequestMessage message, Descriptor descriptor)
             => descriptor.Log[message.LogLength].Term != message.Entries[0].Term ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> AreThereEntriesToAdd(LogRequestMessage message, Descriptor descriptor)
             => message.LogLength + message.Entries.Length > descriptor.Log.Length ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");

        public static Either<string, Descriptor> AreThereUncommitedMessages(LogRequestMessage message, Descriptor descriptor)
             => message.CommitLength > descriptor.CommitLenght ?
                Right<string, Descriptor>(descriptor) :
                Left<string, Descriptor>("");
    }
}

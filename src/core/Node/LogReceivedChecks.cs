using RaftCore.Models;
using TinyFp;

namespace RaftCore.Node
{
    public static class LogReceivedChecks
    {
        public static Either<string, Descriptor> IsTermGreater(LogRequestMessage message, Descriptor descriptor)
            => message.Term > descriptor.CurrentTerm ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsLengthOk(LogRequestMessage message, Descriptor descriptor)
             => descriptor.Log.Length >= message.LogLength && message.LogLength > 0 ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsTermOk(LogRequestMessage message, Descriptor descriptor)
             => message.LogTerm == descriptor.Log[message.LogLength - 1].Term ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsCurrentTermOk(LogRequestMessage message, Descriptor descriptor)
             => message.Term == descriptor.CurrentTerm ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsEntriesLogLengthOk(LogRequestMessage message, Descriptor descriptor)
             => message.Entries.Length > 0 && descriptor.Log.Length > message.LogLength ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> IsEntriesTermhNotOk(LogRequestMessage message, Descriptor descriptor)
             => descriptor.Log[message.LogLength].Term != message.Entries[0].Term ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> AreThereEntriesToAdd(LogRequestMessage message, Descriptor descriptor)
             => message.LogLength + message.Entries.Length > descriptor.Log.Length ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");

        public static Either<string, Descriptor> AreThereUncommitedMessages(LogRequestMessage message, Descriptor descriptor)
             => message.CommitLength > descriptor.CommitLenght ?
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");
    }
}

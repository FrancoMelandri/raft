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
    }
}

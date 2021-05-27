using RaftCore.Models;
using TinyFp;
using TinyFp.Extensions;

namespace RaftCore.Node
{
    public static class Checks
    {
        public static Either<string, int> IsLeader(Descriptor descriptor)
            => descriptor.CurrentRole.ToEither(_ => 0, _ => _ != States.Leader, "");

        public static Either<string, Descriptor> IsTermGreater(LogRequestMessage message,  Descriptor descriptor)
            => message.Term > descriptor.CurrentTerm ? 
                Either<string, Descriptor>.Right(descriptor) :
                Either<string, Descriptor>.Left("");
    }
}

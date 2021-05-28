using TinyFp;
using TinyFp.Extensions;

namespace RaftCore.Node
{
    public static class Checks
    {
        public static Either<string, Descriptor> IsLeader(Descriptor descriptor)
            => descriptor.CurrentRole.ToEither(_ => descriptor, _ => _ != States.Leader, "");
    }
}

using TinyFp;
using TinyFp.Extensions;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class Checks
    {
        public static Either<string, Status> IsLeader(Status descriptor)
            => descriptor.CurrentRole.ToEither(_ => descriptor, _ => _ != States.Leader, "");

        public static Either<string, Status> IsApplicationToBeNotified(Status descriptor, int ready)
            => ready > 0 &&
                ready > descriptor.CommitLenght &&
                descriptor.Log[ready - 1].Term == descriptor.CurrentTerm ?
                Right<string, Status>(descriptor) :
                Left<string, Status>("");
    }
}

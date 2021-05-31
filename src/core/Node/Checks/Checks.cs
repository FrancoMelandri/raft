using TinyFp;
using TinyFp.Extensions;
using static TinyFp.Prelude;

namespace RaftCore.Node
{
    public static class Checks
    {
        public static Either<string, Status> IsLeader(Status status)
            => status.CurrentRole.ToEither(_ => status, _ => _ != States.Leader, "");

        public static Either<string, Status> IsApplicationToBeNotified(Status status, int ready)
            => ready > 0 &&
                ready > status.CommitLenght &&
                status.Log[ready - 1].Term == status.CurrentTerm ?
                Right<string, Status>(status) :
                Left<string, Status>("");
    }
}

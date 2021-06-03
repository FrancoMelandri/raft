using RaftCore.Node;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IStatusRepository
    {
        Option<Status> Load();
        Option<Unit> Save(Status status);
    }
}

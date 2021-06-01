using RaftCore.Node;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IStatusRepository
    {
        Option<Status> LoadStatus();
        Option<Unit> SaveStatus(Status status);
    }
}

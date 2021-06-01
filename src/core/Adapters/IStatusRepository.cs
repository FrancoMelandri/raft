using RaftCore.Node;
using TinyFp;

namespace RaftCore.Adapters
{
    public interface IStatusRepository
    {
        Option<Status> LoadStatus();
        Unit SaveStatus(Status status);
    }
}

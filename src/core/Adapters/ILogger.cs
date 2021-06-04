using TinyFp;

namespace RaftCore.Adapters
{
    public interface ILogger
    {
        Unit Information(string message);
        Unit Error(string message);
    }
}

using TinyFp;

namespace RaftCore.Adapters
{
    public interface IMessageListener
    {
        Unit Start();
        Unit Stop();
    }
}

using RaftCore.Models;

namespace RaftCore.Adapters
{
    public interface IMessageSerializer
    {
        Message Deserialize(int type, string message);
        string Serialize(Message message);
    }
}

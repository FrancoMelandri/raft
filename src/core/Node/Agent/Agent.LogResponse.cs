using RaftCore.Models;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogResponse(LogResponseMessage message)
            => _descriptor;
    }
}

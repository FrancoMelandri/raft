using RaftCore.Models;

namespace RaftCore.Node
{
    public partial class Agent
    {
        public Descriptor OnReceivedLogRequest(LogRequestMessage message)
            => _descriptor;
    }
}

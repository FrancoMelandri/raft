using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;

namespace Raft.Cluster
{
    public class ClusterNode : IClusterNode
    {
        private readonly ClusterNodeConfiguration _clusterNodeConfiguration;
        private readonly IMessageSender _messageSender;

        public int Id => _clusterNodeConfiguration.Id;

        public ClusterNode(ClusterNodeConfiguration clusterNodeConfiguration,
                           IMessageSender messageSender)
        {
            _clusterNodeConfiguration = clusterNodeConfiguration;
            _messageSender = messageSender;
        }

        public Unit SendMessage(Message message)
            => _messageSender.SendMessage(Id, message);
    }
}

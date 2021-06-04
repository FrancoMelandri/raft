using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp.Extensions;

namespace RaftCore.Node
{
    public partial class Agent : IAgent
    {
        private Status _status;
        private readonly ICluster _cluster;
        private readonly IElection _election;
        private readonly ILeader _leader;
        private readonly IApplication _application;
        private BaseNodeConfiguration _nodeConfiguration;

        public Agent(ICluster cluster, 
                     IElection election,
                     ILeader leader,
                     IApplication application)
        {
            _cluster = cluster;
            _election = election;
            _leader = leader;
            _application = application;
        }

        public static Agent Create(ICluster cluster, 
                                   IElection election,
                                   ILeader leader,
                                   IApplication application)
            => new(cluster, election, leader, application);

        public Status CurrentStatus()
            => new Status()
                .Tee(status => status = _status);
    }
}

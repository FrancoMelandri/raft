using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using TinyFp;
using TinyFp.Extensions;
using System.Text.Json;
using static TinyFp.Prelude;
using static System.IO.File;

namespace Raft.Node
{
    public class ClusterNode : INode
    {
        private readonly NodeConfiguration _nodeConfiguration;
        private readonly IAgent _agent;

        public int Id => _nodeConfiguration.Id;

        public ClusterNode(NodeConfiguration nodeConfiguration,
                           IAgent agent)
        {
            _nodeConfiguration = nodeConfiguration;
            _agent = agent;
        }

        public Unit Initialise()
            => GetFileContent(_nodeConfiguration.StatusFileName)
                .Match(status => _agent.OnInitialise(_nodeConfiguration, status),
                       () => _agent.OnInitialise(_nodeConfiguration))
                .Map(_ => Unit.Default);

        private Option<Status> GetFileContent(string fileName)
            => Try(() => Exists(fileName)
                            .Map(_ => ReadAllText(fileName))
                            .Map(_ => JsonSerializer.Deserialize<Status>(_)))
               .Match(_ => Option<Status>.Some(_),
                      _ => Option<Status>.None());
    }
}

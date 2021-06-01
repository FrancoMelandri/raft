using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using TinyFp;
using TinyFp.Extensions;
using System.Text.Json;
using static TinyFp.Prelude;
using static System.IO.File;
using RaftCore.Adapters;

namespace Raft.Node
{
    public class LocalNode : ILocalNode
    {
        private readonly LocalNodeConfiguration _nodeConfiguration;
        private readonly IAgent _agent;
        private readonly IStatusRepository _statusRepository;

        public int Id => _nodeConfiguration.Id;

        public LocalNode(LocalNodeConfiguration nodeConfiguration,
                         IAgent agent,
                         IStatusRepository statusRepository)
        {
            _nodeConfiguration = nodeConfiguration;
            _agent = agent;
            _statusRepository = statusRepository;
        }

        public Unit Initialise()
            => LoadStatusFromFile(_nodeConfiguration.StatusFileName)
                .Match(status => _agent.OnInitialise(_nodeConfiguration, status),
                       () => _agent.OnInitialise(_nodeConfiguration))
                .Map(_ => Unit.Default);

        public Unit Deinitialise()
            => Unit.Default;

        private static Option<Status> LoadStatusFromFile(string fileName)
            => Try(() => Exists(fileName)
                            .Map(_ => ReadAllText(fileName))
                            .Map(_ => JsonSerializer.Deserialize<Status>(_)))
               .Match(_ => Option<Status>.Some(_),
                      _ => Option<Status>.None());

        private Option<Status> SaveStatusToFile(string fileName)
            => Try(() => JsonSerializer.Serialize<Status>(_agent.CurrentStatus())
                            .Map(_ => ReadAllText(fileName))
                            .Map(_ => JsonSerializer.Deserialize<Status>(_)))
               .Match(_ => Option<Status>.Some(_),
                      _ => Option<Status>.None());
    }
}

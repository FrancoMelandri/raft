using RaftCore.Adapters;
using RaftCore.Node;
using System.Diagnostics.CodeAnalysis;
using TinyFp;
using TinyFp.Extensions;
using System.Text.Json;
using static TinyFp.Prelude;
using static System.IO.File;

namespace Raft.Node
{
    [ExcludeFromCodeCoverage]
    public class FileStatusRepository : IStatusRepository
    {
        private readonly LocalNodeConfiguration _localNodeConfiguration;

        public FileStatusRepository(LocalNodeConfiguration localNodeConfiguration)
        {
            _localNodeConfiguration = localNodeConfiguration;
        }

        public Option<Status> Load()
            => Try(() => Exists(_localNodeConfiguration.StatusFileName)
                            .ToOption(_ => !_)
                            .Map(_ => ReadAllText(_localNodeConfiguration.StatusFileName))
                            .Map(_ => JsonSerializer.Deserialize<Status>(_)))
               .Match(_ => _,
                      _ => Option<Status>.None());

        public Option<Unit> Save(Status status)
            => Try(() => JsonSerializer.Serialize(status)
                            .Tee(_ => WriteAllText(_localNodeConfiguration.StatusFileName, _)))
               .Match(_ => Option<Unit>.Some(Unit.Default),
                      _ => Option<Unit>.None());
    }
}

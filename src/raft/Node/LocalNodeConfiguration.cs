using RaftCore.Models;
using System.Diagnostics.CodeAnalysis;

namespace Raft.Node
{

    [ExcludeFromCodeCoverage]
    public record LocalNodeConfiguration : NodeConfiguration
    {
        public string StatusFileName { get; set; }
    }
}

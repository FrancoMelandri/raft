using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{
    [ExcludeFromCodeCoverage]
    public record ClusterConfiguration
    {
        public NodeConfiguration[] Nodes { get; set; }
    }
}

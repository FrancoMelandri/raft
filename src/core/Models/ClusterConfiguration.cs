using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{
    [ExcludeFromCodeCoverage]
    public record ClusterConfiguration
    {
        public ClusterNodeConfiguration[] Nodes { get; set; }
    }
}

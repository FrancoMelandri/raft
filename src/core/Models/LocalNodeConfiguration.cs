using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{

    [ExcludeFromCodeCoverage]
    public record LocalNodeConfiguration : NodeConfiguration
    {
        public string StatusFileName { get; set; }
    }
}

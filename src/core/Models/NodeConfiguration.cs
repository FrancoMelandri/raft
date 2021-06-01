using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{
    [ExcludeFromCodeCoverage]
    public record NodeConfiguration : BaseNodeConfiguration
    {
        public Dictionary<string, string> Properties { get; set; }
    }
}

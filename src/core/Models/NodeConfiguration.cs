using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{
    [ExcludeFromCodeCoverage]
    public record NodeConfiguration
    {
        public int Id { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}

using System.Diagnostics.CodeAnalysis;

namespace RaftCore.Models
{
    [ExcludeFromCodeCoverage]
    public record BaseNodeConfiguration
    {
        public int Id { get; set; }
    }
}

using System.Collections.Generic;

namespace RaftCore.Models
{
    public record NodeConfiguration
    {
        public int Id { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}

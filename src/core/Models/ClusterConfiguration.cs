namespace RaftCore.Models
{
    public record ClusterConfiguration
    {
        public NodeConfiguration[] Nodes { get; set; }
    }
}

namespace RaftCore.Models
{
    public record LogResponseMessage : Message
    {
        public int NodeId { get; set; }
        public int Term { get; set; }
        public int Ack { get; set; }
        public bool Success { get; set; }
    }
}

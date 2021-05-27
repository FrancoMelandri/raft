namespace RaftCore.Models
{
    public record LogResponseMessage : Message
    {
        public int NodeId { get; set; }
        public int CurrentTerm { get; set; }
        public int Length { get; set; }
        public bool Ack { get; set; }
    }
}

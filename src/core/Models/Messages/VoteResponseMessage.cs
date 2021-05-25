namespace RaftCore.Models
{
    public record VoteResponseMessage : Message
    {
        public int NodeId { get; set; }
        public int CurrentTerm { get; set; }
        public bool Granted { get; set; }
    }
}

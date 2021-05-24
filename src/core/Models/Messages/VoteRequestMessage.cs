namespace RaftCore.Models
{
    public record VoteRequestMessage : Message
    {
        public int NodeId { get; set; }
        public int CurrentTerm { get; set; }
        public int LogLength { get; set; }
        public int LastTerm { get; set; }
    }
}

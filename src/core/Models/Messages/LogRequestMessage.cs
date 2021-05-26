namespace RaftCore.Models
{
    public record LogRequestMessage : Message
    {
        public int LeaderId { get; set; }
        public int CurrentTerm { get; set; }
        public int StartLength { get; set; }
        public int PrevTerm { get; set; }
        public int CommitLength { get; set; }
        public LogEntry[] Entries { get; set; }
    }
}

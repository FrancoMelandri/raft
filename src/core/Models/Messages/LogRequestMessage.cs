namespace RaftCore.Models
{
    public record LogRequestMessage : Message
    {
        public int LeaderId { get; set; }
        public int CurrentTerm { get; set; }
        public int LogLength { get; set; }
        public int LogTerm { get; set; }
        public int CommitLength { get; set; }
        public LogEntry[] Entries { get; set; }
    }
}

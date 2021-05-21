namespace RaftCore.Models
{
    public struct LogEntry
    {
        public Message Message { get; set; }
        public int Term { get; set; }
    }
}

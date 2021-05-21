namespace RaftCore.Models
{
    public struct Message
    {
        public MessageType Type { get; set; }
        public int NodeId { get; set; }
        public int CurrentTerm { get; set; }
        public int LogLength { get; set; }
        public int LastTerm { get; set; }
    }
}

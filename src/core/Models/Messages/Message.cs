namespace RaftCore.Models
{
    public record Message
    {
        public MessageType Type { get; set; }
    }
}

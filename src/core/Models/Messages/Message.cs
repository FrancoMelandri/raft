namespace RaftCore.Models
{
    public record Message
    {
        public MessageType Type { get; set; }

        internal static Message Empty
            => new()
                {
                    Type = MessageType.None
                };
    }
}

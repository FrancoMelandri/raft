using RaftCore.Models;

namespace RaftApplication.Messages
{
    public record ApplicationMessage : Message
    {
        public string CurrentStatus { get; set; }
    }
}

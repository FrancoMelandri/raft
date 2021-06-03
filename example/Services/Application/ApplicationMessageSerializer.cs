using Raft.Node;
using RaftApplication.Messages;
using RaftCore.Models;
using System.Text.Json;

namespace RaftApplication.Services.Application
{
    public class ApplicationMessageSerializer : MessageSerializer
    {
        protected override Message ApplicationMessageDeserialize(string message)
            => JsonSerializer.Deserialize<ApplicationMessage>(message);
    }
}

using System.Diagnostics.CodeAnalysis;

namespace Raft.Constants
{
    [ExcludeFromCodeCoverage]
    public class Logs
    {
        public static readonly string ERROR_CONNECTING_TO_NODE = "Error connectiong to node";
        public static readonly string ERROR_SENDING_MESSAGE = "Error sending message";
    }
}

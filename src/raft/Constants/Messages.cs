using System.Diagnostics.CodeAnalysis;

namespace Raft.Constants
{
    [ExcludeFromCodeCoverage]
    public static class Messages
    {
        public static readonly int HEADER_SIZE = 16;
        public static readonly string PADDING_CHAR = " ";
        public static readonly string REPLACING_CHAR = "";
    }
}

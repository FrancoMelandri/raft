using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp.Extensions;
using static System.Math;

namespace RaftCore
{
    public static class Utils
    {
        public static int LastEntryOrZero(LogEntry[] Log)
            => Log
                .ToOption(_ => _.Length == 0)
                .Map(_ => _[^1].Term)
                .OnNone(0);

        public static int GetQuorum(ICluster cluster)
            => (int)Floor(((decimal)cluster.Nodes.Length + 1) / 2);
    }
}

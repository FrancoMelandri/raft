using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using Raft.Cluster;

namespace RaftTest.Raft
{
    [TestFixture]
    public class ClusterNodeTest
    {
        [Test]
        public void Ctor_SetRightId()
        {
            var config = new ClusterNodeConfiguration
            {
                Id = 1
            };

            new ClusterNode(config)
                .Id.Should().Be(1);
        }
    }
}

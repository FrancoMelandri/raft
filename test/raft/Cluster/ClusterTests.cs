using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raft.Cluster;
using RaftCore.Cluster;
using RaftCore.Models;
using TinyFp;

namespace RaftTest.Raft
{
    [TestFixture]
    public class ClusterTests
    {
        private Cluster _sut;
        private Mock<INode> _node1;
        private Mock<INode> _node2;
        private Mock<INode> _node3;

        [SetUp]
        public void SetUp()
        {
            _node1 = new Mock<INode>();
            _node2 = new Mock<INode>();
            _node3 = new Mock<INode>();

            _sut = new Cluster(new[] { _node1.Object,
                                       _node2.Object,
                                       _node3.Object });
        }

        [Test]
        public void Cluster_HasCorrectNodes()
            => _sut.Nodes.Should().HaveCount(3);

        [Test]
        public void SendBroadcastMessage_DoNothing()
            => _sut.SendBroadcastMessage(new Message())
                .Should().Be(Unit.Default);

        [Test]
        public void SendMessage_DoNothing()
            => _sut.SendMessage(1, new Message())
                .Should().Be(Unit.Default);
    }
}

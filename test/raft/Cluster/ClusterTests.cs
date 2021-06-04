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
        private Mock<IClusterNode> _node1;
        private Mock<IClusterNode> _node2;
        private Mock<IClusterNode> _node3;

        [SetUp]
        public void SetUp()
        {
            _node1 = new Mock<IClusterNode>();
            _node1.Setup(m => m.Id).Returns(1);
            _node2 = new Mock<IClusterNode>();
            _node2.Setup(m => m.Id).Returns(2);
            _node3 = new Mock<IClusterNode>();
            _node3.Setup(m => m.Id).Returns(3);

            _sut = new Cluster(new[] { _node1.Object,
                                       _node2.Object,
                                       _node3.Object });
        }

        [Test]
        public void Cluster_HasCorrectNodes()
            => _sut.Nodes.Should().HaveCount(3);

        [Test]
        public void SendBroadcastMessage_SendMessageToAllNodes()
        {
            var message = new Message();
            _ = _sut.SendBroadcastMessage(message);

            _node1
                .Verify(m => m.SendMessage(message), Times.Once);
            _node2
                .Verify(m => m.SendMessage(message), Times.Once);
            _node3
                .Verify(m => m.SendMessage(message), Times.Once);
        }

        [Test]
        public void SendMessage_SendMessageToSingleNode()
        {
            var message = new Message();
            _ = _sut.SendMessage(2, message);

            _node1
                .Verify(m => m.SendMessage(message), Times.Never);
            _node2
                .Verify(m => m.SendMessage(message), Times.Once);
            _node3
                .Verify(m => m.SendMessage(message), Times.Never);
        }

        [Test]
        public void SendMessage_WhenNodeDoesnotExists_DoNothing()
        {
            var message = new Message();
            _ = _sut.SendMessage(99, message);

            _node1
                .Verify(m => m.SendMessage(message), Times.Never);
            _node2
                .Verify(m => m.SendMessage(message), Times.Never);
            _node3
                .Verify(m => m.SendMessage(message), Times.Never);
        }
    }
}

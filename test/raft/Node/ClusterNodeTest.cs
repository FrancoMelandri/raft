using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using Raft.Cluster;
using Moq;
using RaftCore.Adapters;

namespace RaftTest.Raft
{
    [TestFixture]
    public class ClusterNodeTest
    {
        private ClusterNode _sut;
        private Mock<IMessageSender> _messageSender;
        private ClusterNodeConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _config = new ClusterNodeConfiguration
            {
                Id = 1
            };
            _messageSender = new Mock<IMessageSender>();
            _sut = new ClusterNode(_config,
                                   _messageSender.Object);
        }

        [Test]
        public void Ctor_SetRightId()
        {
            _sut.Id.Should().Be(1);
            _messageSender
                .Verify(m => m.Start(1), Times.Once);
        }

        [Test]
        public void SendMessage_CallMessageZSender()
        {
            var message = new Message();

            _sut.SendMessage(message);

            _messageSender
                .Verify(m => m.SendMessage(message), Times.Once);
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Models;

namespace RaftCoreTest
{
    [TestFixture]
    public class NodeTests
    {
        private Node _sut;
        private NodeConfiguration  _nodeConfiguration;

        [SetUp]
        public void SetUp()
        {
            _nodeConfiguration = new NodeConfiguration
            {
                Id = 1
            };
            _sut = new Node(_nodeConfiguration);
        }

        [Test]
        public void Node_HaCorrectId()
            => _sut.Id.Should().Be(1);
    }
}

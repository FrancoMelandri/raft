using FluentAssertions;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Models;

namespace RaftTest
{
    [TestFixture]
    public class RaftNodeTests
    {
        private ClusterNode _sut;
        private NodeConfiguration _nodeConfiguration;

        [SetUp]
        public void SetUp()
        {
            _nodeConfiguration = new NodeConfiguration
            {
                Id = 1
            };
            _sut = new ClusterNode(_nodeConfiguration);
        }

        [Test]
        public void ClusterNode_HasCorrectId()
            => _sut.Id.Should().Be(1);
    }
}

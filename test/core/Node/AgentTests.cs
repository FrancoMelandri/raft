using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class AgentTests
    {
        private Agent _sut;
        private Mock<ICluster> _cluster;
        private Mock<IElection> _election;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _election = new Mock<IElection>();
            _sut = Agent.Create(_cluster.Object,
                                _election.Object);
        }

        [Test]
        public void OnInitialise_SetTheRightValues()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };
            var descriptor = _sut.OnInitialise(nodeConfig);

            _sut.Configuration.Id.Should().Be(42);
            descriptor.CurrentTerm.Should().Be(0);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.Log.Should().BeEquivalentTo(new LogEntry[] { });
            descriptor.CommitLenght.Should().Be(0);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.CurrentLeader.Should().Be(-1);
            descriptor.VotesReceived.Should().NotBeNull();
            descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }

        [Test]
        public void OnRecoverFromCrash_SetTheRightValues()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 42,
                VotedFor = 42,
                Log = new LogEntry[] { new LogEntry() },
                CommitLenght = 42,
                CurrentRole = States.Leader,
                CurrentLeader = 42,
                VotesReceived = null,
                SentLength = new Dictionary<int, int> { { 1, 1} },
                AckedLength = new Dictionary<int, int> { { 1, 1 } }
            };

            descriptor = _sut.OnRecoverFromCrash(nodeConfig, descriptor);

            _sut.Configuration.Id.Should().Be(42);
            descriptor.CurrentTerm.Should().Be(42);
            descriptor.VotedFor.Should().Be(42);
            descriptor.Log.Length.Should().Be(1);
            descriptor.CommitLenght.Should().Be(42);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.CurrentLeader.Should().Be(-1);
            descriptor.VotesReceived.Should().NotBeNull();
            descriptor.SentLength.Should().BeEquivalentTo(new Dictionary<int, int>());
            descriptor.AckedLength.Should().BeEquivalentTo(new Dictionary<int, int>());
        }
    }
}

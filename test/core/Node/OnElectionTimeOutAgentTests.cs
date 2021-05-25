using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnElectionTimeOutAgentTests
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
        public void WhenEmptyLog_SendMessage_VoteRequest_And_LastTermZero()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 42,
                VotedFor = 42,
                Log = new LogEntry[] {  },
                CommitLenght = 42,
                CurrentRole = States.Leader,
                CurrentLeader = 42,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            agent.OnLedaerHasFailed();

            agent.Descriptor.CurrentTerm.Should().Be(43);
            agent.Descriptor.CurrentRole.Should().Be(States.Candidate);
            agent.Descriptor.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 0 &&
                                                    p.LogLength == 0 &&
                                                    p.CurrentTerm == 43)), Times.Once);
        }

        [Test]
        public void WhenLogNotEmpty_SendMessage_VoteRequest_And_LastTerm()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 42,
                VotedFor = 42,
                Log = new LogEntry[] { new LogEntry { Term = 10} },
                CommitLenght = 42,
                CurrentRole = States.Leader,
                CurrentLeader = 42,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            agent.OnLedaerHasFailed();

            agent.Descriptor.CurrentTerm.Should().Be(43);
            agent.Descriptor.CurrentRole.Should().Be(States.Candidate);
            agent.Descriptor.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 10 &&
                                                    p.LogLength == 1 &&
                                                    p.CurrentTerm == 43)), Times.Once);
        }
    }
}

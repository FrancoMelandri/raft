using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{

    [TestFixture]
    public class AgentTests
    {
        private Agent _sut;
        private Mock<ICluster> _cluster;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _sut = Agent.Create(_cluster.Object);
        }

        [Test]
        public void OnInitialise_SetTheRightValues()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };
            var agent = _sut.OnInitialise(nodeConfig);

            agent.Configuration.Id.Should().Be(42);
            agent.Descriptor.CurrentTerm.Should().Be(0);
            agent.Descriptor.VotedFor.Should().Be(-1);
            agent.Descriptor.Log.Should().BeEquivalentTo(new LogEntry[] { });
            agent.Descriptor.CommitLenght.Should().Be(0);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
            agent.Descriptor.CurrentLeader.Should().Be(-1);
            agent.Descriptor.VotesReceived.Should().NotBeNull();
            agent.Descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
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
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);

            agent.Configuration.Id.Should().Be(42);
            agent.Descriptor.CurrentTerm.Should().Be(42);
            agent.Descriptor.VotedFor.Should().Be(42);
            agent.Descriptor.Log.Length.Should().Be(1);
            agent.Descriptor.CommitLenght.Should().Be(42);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
            agent.Descriptor.CurrentLeader.Should().Be(-1);
            agent.Descriptor.VotesReceived.Should().NotBeNull();
            agent.Descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }

        [Test]
        public void OnLedaerHasFailed_WhenEmptyLog_SendMessage_VoteRequest_And_LastTermZero()
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
                .Verify(m => m.SendMessage(It.Is<VoteRequestMessage>(
                                                p => p.NodeId == 42 &&
                                                p.LastTerm == 0 &&
                                                p.LogLength == 0 &&
                                                p.CurrentTerm == 43)), Times.Once);
        }

        [Test]
        public void OnLedaerHasFailed_WhenLogNotEmpty_SendMessage_VoteRequest_And_LastTerm()
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
                .Verify(m => m.SendMessage(It.Is<VoteRequestMessage>(
                                                p => p.NodeId == 42 &&
                                                p.LastTerm == 10 &&
                                                p.LogLength == 1 &&
                                                p.CurrentTerm == 43)), Times.Once);
        }

        [Test]
        public void OnElectionTimeOut_WhenEmptyLog_SendMessage_VoteRequest_And_LastTermZero()
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
                .Verify(m => m.SendMessage(It.Is<VoteRequestMessage>(
                                                p => p.NodeId == 42 &&
                                                p.LastTerm == 0 &&
                                                p.LogLength == 0 &&
                                                p.CurrentTerm == 43)), Times.Once);
        }

        [Test]
        public void OnElectionTimeOut_WhenLogNotEmpty_SendMessage_VoteRequest_And_LastTerm()
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
                .Verify(m => m.SendMessage(It.Is<VoteRequestMessage>(
                                                p => p.NodeId == 42 &&
                                                p.LastTerm == 10 &&
                                                p.LogLength == 1 &&
                                                p.CurrentTerm == 43)), Times.Once);
        }
    }
}

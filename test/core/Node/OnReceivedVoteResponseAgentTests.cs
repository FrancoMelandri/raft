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
    public class OnReceivedVoteResponseAgentTests
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
        public void CurrentRole_Not_Candidate_And_Term_LessOrEqual_CurrentTerm_DoNothing()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int> (),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 10,
                Granted = false
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(11);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.VotedFor.Should().Be(1);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().BeEmpty();
            _election
                .Verify(m => m.Cancel(), Times.Never);
        }

        [Test]
        public void CurrentRole_Not_Candidate_And_Term_GreaterThan_CurrentTerm_UpdateDesccriptor()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 12,
                Granted = false
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(12);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().BeEmpty();
            _election
                .Verify(m => m.Cancel(), Times.Once);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Not_Granted_DoNothing()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 10,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(11);
            descriptor.CurrentRole.Should().Be(States.Candidate);
            descriptor.VotedFor.Should().Be(42);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().HaveCount(1);
            _election
                .Verify(m => m.Cancel(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Different_CurrentTerm_And_Not_Granted_DoNothing()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(12);
            descriptor.CurrentRole.Should().Be(States.Candidate);
            descriptor.VotedFor.Should().Be(42);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().HaveCount(1);
            _election
                .Verify(m => m.Cancel(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Granted_WithNoQuorum_UpdateVotereceived()
        {
            _cluster
                .Setup(m => m.Nodes)
                .Returns(new INode[] { null, null, null, null, null });
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 10,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = true
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(11);
            descriptor.CurrentRole.Should().Be(States.Candidate);
            descriptor.VotedFor.Should().Be(42);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().Contain(42).And.Contain(99);
            _election
                .Verify(m => m.Cancel(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Granted_WithQuorum_PromoteAsLeader()
        {
            var node1 = new Mock<INode>();
            node1.Setup(m => m.Id).Returns(1);
            var node2 = new Mock<INode>();
            node2.Setup(m => m.Id).Returns(2);
            var node3 = new Mock<INode>();
            node3.Setup(m => m.Id).Returns(99);

            _cluster
                .Setup(m => m.Nodes)
                .Returns(new INode[] { node1.Object, node2.Object, node3.Object });

            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 10,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 9 }, new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 1,
                CurrentTerm = 11,
                Granted = true
            };
            descriptor = _sut.OnReceivedVoteResponse(message);
            message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 2,
                CurrentTerm = 11,
                Granted = true
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(11);
            descriptor.CurrentRole.Should().Be(States.Leader);
            descriptor.CurrentLeader.Should().Be(42);
            descriptor.VotedFor.Should().Be(42);
            descriptor.SentLength[1].Should().Be(2);
            descriptor.SentLength[2].Should().Be(2);
            descriptor.AckedLength[1].Should().Be(0);
            descriptor.AckedLength[2].Should().Be(0);
            descriptor.VotesReceived.Should().Contain(42)
                                              .And.Contain(1)
                                              .And.Contain(2);

            _election
                .Verify(m => m.Cancel(), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 1), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 2), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 42), Times.Never);
        }
    }
}

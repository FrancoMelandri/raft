using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnReceivedVoteResponseAgentTests : BaseUseCases
    {
        [Test]
        public void CurrentRole_Not_Candidate_And_Term_LessOrEqual_CurrentTerm_DoNothing()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            _ = _sut.OnInitialise(nodeConfig, status);
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 10,
                Granted = false
            };
            status = _sut.OnReceivedVoteResponse(message);

            status.CurrentTerm.Should().Be(11);
            status.CurrentRole.Should().Be(States.Follower);
            status.VotedFor.Should().Be(1);
            status.SentLength.Should().BeEmpty();
            status.AckedLength.Should().BeEmpty();
            status.VotesReceived.Should().BeEmpty();
            _election
                .Verify(m => m.Stop(), Times.Never);
            _leader
                .Verify(m => m.Stop(), Times.Never);
        }

        [Test]
        public void CurrentRole_Not_Candidate_And_Term_GreaterThan_CurrentTerm_UpdateDesccriptor()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            _ = _sut.OnInitialise(nodeConfig, status);
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 12,
                Granted = false
            };
            status = _sut.OnReceivedVoteResponse(message);

            status.CurrentTerm.Should().Be(12);
            status.CurrentRole.Should().Be(States.Follower);
            status.VotedFor.Should().Be(-1);
            status.SentLength.Should().BeEmpty();
            status.AckedLength.Should().BeEmpty();
            status.VotesReceived.Should().BeEmpty();
            _election
                .Verify(m => m.Stop(), Times.Once);
            _leader
                .Verify(m => m.Stop(), Times.Once);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Not_Granted_DoNothing()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            _ = _sut.OnInitialise(nodeConfig, status);
            status = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };
            status = _sut.OnReceivedVoteResponse(message);

            status.CurrentTerm.Should().Be(11);
            status.CurrentRole.Should().Be(States.Candidate);
            status.VotedFor.Should().Be(42);
            status.SentLength.Should().BeEmpty();
            status.AckedLength.Should().BeEmpty();
            status.VotesReceived.Should().HaveCount(1);
            _election
                .Verify(m => m.Stop(), Times.Never);
            _leader
                .Verify(m => m.Stop(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Different_CurrentTerm_And_Not_Granted_DoNothing()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            _ = _sut.OnInitialise(nodeConfig, status);
            status = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };
            status = _sut.OnReceivedVoteResponse(message);

            status.CurrentTerm.Should().Be(12);
            status.CurrentRole.Should().Be(States.Candidate);
            status.VotedFor.Should().Be(42);
            status.SentLength.Should().BeEmpty();
            status.AckedLength.Should().BeEmpty();
            status.VotesReceived.Should().HaveCount(1);
            _election
                .Verify(m => m.Stop(), Times.Never);
            _leader
                .Verify(m => m.Stop(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Granted_WithNoQuorum_UpdateVotereceived()
        {
            _cluster
                .Setup(m => m.Nodes)
                .Returns(new IClusterNode[] { null, null, null, null, null });
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            _ = _sut.OnInitialise(nodeConfig, status);
            status = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = true
            };
            status = _sut.OnReceivedVoteResponse(message);

            status.CurrentTerm.Should().Be(11);
            status.CurrentRole.Should().Be(States.Candidate);
            status.VotedFor.Should().Be(42);
            status.SentLength.Should().BeEmpty();
            status.AckedLength.Should().BeEmpty();
            status.VotesReceived.Should().Contain(42).And.Contain(99);
            _election
                .Verify(m => m.Stop(), Times.Never);
            _leader
                .Verify(m => m.Stop(), Times.Never);
        }

        [Test]
        public void CurrentRole_Candidate_And_Term_Equals_CurrentTerm_And_Granted_WithQuorum_PromoteAsLeader()
        {
            var status = UseNodeAsLeader();

            status.CurrentTerm.Should().Be(11);
            status.CurrentRole.Should().Be(States.Leader);
            status.CurrentLeader.Should().Be(42);
            status.VotedFor.Should().Be(42);
            status.SentLength[1].Should().Be(6);
            status.SentLength[2].Should().Be(6);
            status.AckedLength[1].Should().Be(0);
            status.AckedLength[2].Should().Be(0);
            status.VotesReceived.Should().Contain(42)
                                              .And.Contain(1)
                                              .And.Contain(2);

            _election
                .Verify(m => m.Stop(), Times.Once);
            _leader
                .Verify(m => m.Start(_sut), Times.Once);
            _cluster
                .Verify(m => m.SendMessage(1,
                                            It.Is<LogRequestMessage>(p =>
                                                p.Type == MessageType.LogRequest &&
                                                p.LeaderId == 42 &&
                                                p.LogTerm == 11 &&
                                                p.LogLength == 6)), Times.Once);
            _cluster
                .Verify(m => m.SendMessage(2,
                                            It.Is<LogRequestMessage>(p =>
                                                p.Type == MessageType.LogRequest &&
                                                p.LeaderId == 42 &&
                                                p.LogTerm == 11 &&
                                                p.LogLength == 6)), Times.Once);
            _cluster
                .Verify(m => m.SendMessage(3,
                                            It.Is<LogRequestMessage>(p =>
                                                p.Type == MessageType.LogRequest &&
                                                p.LeaderId == 42 &&
                                                p.LogTerm == 11 &&
                                                p.LogLength == 6)), Times.Once);
            _cluster
                .Verify(m => m.SendMessage(42,
                                            It.IsAny<LogRequestMessage>()), Times.Never);
        }
    }
}

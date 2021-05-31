using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnReceivedVoteRequestAgentTests : BaseUseCases
    {
        [Test]
        public void NodeLogTerm_GreaterThan_CandidateLog_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 9
            };
            descriptor = _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 11 &&
                                                p.Granted == false)), Times.Once);
            descriptor.CurrentTerm.Should().Be(11);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_LessThan_CandidateLog_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 12
            };
            descriptor = _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(99, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.Granted == true)), Times.Once);
            descriptor.CurrentTerm.Should().Be(12);
            descriptor.VotedFor.Should().Be(99);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_Empty_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 12
            };
            descriptor = _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(99, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.Granted == true)), Times.Once);
            descriptor.CurrentTerm.Should().Be(12);
            descriptor.VotedFor.Should().Be(99);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_Equal_CandidateLog_LogLength_Greater_ThanCandidate_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 }, new LogEntry { Term = 11 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 11,
                LogLength = 1
            };
            _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 11 &&
                                                p.Granted == false)), Times.Once);
            descriptor.CurrentTerm.Should().Be(11);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_Equal_CandidateLog_LogLebngth_Equal_ThanCandidate_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 }, new LogEntry { Term = 11 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 11,
                LogLength = 2,
                CurrentTerm = 12
            };
            descriptor = _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(99, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.Granted == true)), Times.Once);
            descriptor.CurrentTerm.Should().Be(12);
            descriptor.VotedFor.Should().Be(99);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void CurrentTerm_Lessthan_CandidateTerm_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 4,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            descriptor = _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(99, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 5 &&
                                                p.Granted == true)), Times.Once);
            descriptor.CurrentTerm.Should().Be(5);
            descriptor.VotedFor.Should().Be(99);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void CurrentTerm_Ok__And_Already_VotedFor_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 6,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 6 &&
                                                p.Granted == false)), Times.Once);
            descriptor.CurrentTerm.Should().Be(6);
            descriptor.VotedFor.Should().Be(1);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void CurrentTerm_Ok_And_Not_VotedFor_RespondWithTrue(int voteFor)
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 6,
                VotedFor = voteFor,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            _sut.OnReceivedVoteRequest(message);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 6 &&
                                                p.Granted == false)), Times.Once);
            descriptor.CurrentTerm.Should().Be(6);
            descriptor.VotedFor.Should().Be(voteFor);
            descriptor.CurrentRole.Should().Be(States.Follower);
        }
    }
}

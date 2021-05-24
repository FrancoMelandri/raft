﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedVotedRequestAgentTests
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
        public void NodeLogTerm_GreaterThan_CandidateLog_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                LastTerm = 9
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 11 &&
                                                p.InFavour == false)), Times.Once);
        }

        [Test]
        public void NodeLogTerm_LessThan_CandidateLog_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 12
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.InFavour == true)), Times.Once);
            agent.Descriptor.CurrentTerm.Should().Be(12);
            agent.Descriptor.VotedFor.Should().Be(99);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_Empty_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 12
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.InFavour == true)), Times.Once);
            agent.Descriptor.CurrentTerm.Should().Be(12);
            agent.Descriptor.VotedFor.Should().Be(99);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void NodeLogTerm_Equal_CandidateLog_LogLength_Greater_ThanCandidate_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 }, new LogEntry { Term = 11 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                LastTerm = 11,
                LogLength = 1
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 11 &&
                                                p.InFavour == false)), Times.Once);
        }

        [Test]
        public void NodeLogTerm_Equal_CandidateLog_LogLebngth_Equal_ThanCandidate_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 }, new LogEntry { Term = 11 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 11,
                LogLength = 2,
                CurrentTerm = 12
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 12 &&
                                                p.InFavour == true)), Times.Once);
            agent.Descriptor.CurrentTerm.Should().Be(12);
            agent.Descriptor.VotedFor.Should().Be(99);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void CurrentTerm_Lessthan_CandidateTerm_RespondWithTrue()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 4,
                VotedFor = -1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 5 &&
                                                p.InFavour == true)), Times.Once);
            agent.Descriptor.CurrentTerm.Should().Be(5);
            agent.Descriptor.VotedFor.Should().Be(99);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
        }

        [Test]
        public void CurrentTerm_Ok__And_Already_VotedFor_RespondWithFalse()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 6,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 6 &&
                                                p.InFavour == false)), Times.Once);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void CurrentTerm_Ok_And_Not_VotedFor_RespondWithTrue(int voteFor)
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 6,
                VotedFor = voteFor,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 1,
                LastTerm = 12,
                LogLength = 1,
                CurrentTerm = 5
            };
            agent.OnReceivedVotedRequest(message);

            _cluster
                .Verify(m => m.SendMessage(It.Is<VoteResponseMessage>(
                                                p => p.NodeId == 42 &&
                                                p.CurrentTerm == 6 &&
                                                p.InFavour == false)), Times.Once);
            agent.Descriptor.CurrentTerm.Should().Be(6);
            agent.Descriptor.VotedFor.Should().Be(voteFor);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
        }
    }
}

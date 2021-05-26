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
    public class OnLedaerHasFailedAgentTests : BaseUseCases
    {
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
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();

            descriptor.CurrentTerm.Should().Be(43);
            descriptor.CurrentRole.Should().Be(States.Candidate);
            descriptor.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 0 &&
                                                    p.LogLength == 0 &&
                                                    p.CurrentTerm == 43)), Times.Once);
            _election
                .Verify(m => m.Start(), Times.Once);
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
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();

            descriptor.CurrentTerm.Should().Be(43);
            descriptor.CurrentRole.Should().Be(States.Candidate);
            descriptor.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 10 &&
                                                    p.LogLength == 1 &&
                                                    p.CurrentTerm == 43)), Times.Once);
            _election
                .Verify(m => m.Start(), Times.Once);
        }
    }
}

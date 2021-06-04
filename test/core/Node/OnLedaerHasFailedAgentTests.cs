using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{

    [TestFixture]
    public class OnLedaerHasFailedAgentTests : BaseUseCases
    {
        [Test]
        public void WhenEmptyLog_SendMessage_VoteRequest_And_LastTermZero()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            status = _sut.OnInitialise(nodeConfig, status);
            status = _sut.OnLeaderHasFailed();

            status.CurrentTerm.Should().Be(43);
            status.CurrentRole.Should().Be(States.Candidate);
            status.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 0 &&
                                                    p.LogLength == 0 &&
                                                    p.CurrentTerm == 43)), Times.Once);
            _election
                .Verify(m => m.Start(), Times.Once);
            _logger
                .Verify(m => m.Information("Leader has failed"), Times.Once);
        }

        [Test]
        public void WhenLogNotEmpty_SendMessage_VoteRequest_And_LastTerm()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
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

            status = _sut.OnInitialise(nodeConfig, status);
            status = _sut.OnLeaderHasFailed();

            status.CurrentTerm.Should().Be(43);
            status.CurrentRole.Should().Be(States.Candidate);
            status.VotedFor.Should().Be(42);
            _cluster
                .Verify(m => m.SendBroadcastMessage(It.Is<VoteRequestMessage>(
                                                    p => p.NodeId == 42 &&
                                                    p.LastTerm == 10 &&
                                                    p.LogLength == 1 &&
                                                    p.CurrentTerm == 43)), Times.Once);
            _election
                .Verify(m => m.Start(), Times.Once);
            _logger
                .Verify(m => m.Information("Leader has failed"), Times.Once);
        }
    }
}

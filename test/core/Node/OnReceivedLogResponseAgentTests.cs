﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnReceivedLogResponseAgentTests : BaseUseCases
    {
        [Test]
        public void WhenTerm_GreaterThan_CurrentTerm_MoveToFollower()
        {
            _ = UseNodeAsLeader();

            var logResponse = new LogResponseMessage
            {
                Type = MessageType.LogResponse,
                Term = 12,
                NodeId = 1,
                Ack = 3,
                Success = false
            };

            var status = _sut.OnReceivedLogResponse(logResponse);

            status.CurrentRole.Should().Be(States.Follower);
            status.VotedFor.Should().Be(-1);
            status.CurrentTerm.Should().Be(12);
        }

        [Test]
        public void WhenTerm_EqualTo_CurrentTerm_AndNot_Leader_DiscardMessage()
        {
            var status = UseNodeAsFollower();

            var logResponse = new LogResponseMessage
            {
                Type = MessageType.LogResponse,
                Term = 10,
                NodeId = 1,
                Ack = 3,
                Success = false
            };

            var descriptorResult = _sut.OnReceivedLogResponse(logResponse);

            descriptorResult.Should().BeEquivalentTo(status);
        }

        [Test]
        public void WhenTerm_EqualTo_CurrentTerm_And_Leader_AndSuccess_UpdateDescriptorAndCommitEntries()
        {
            _ = UseNodeAsLeader();

            var logResponse = new LogResponseMessage
            {
                Type = MessageType.LogResponse,
                Term = 11,
                NodeId = 1,
                Ack = 3,
                Success = true
            };

            var status = _sut.OnReceivedLogResponse(logResponse);

            status.SentLength[1].Should().Be(3);
            status.AckedLength[1].Should().Be(3);
        }

        [Test]
        public void WhenTerm_EqualTo_CurrentTerm_And_Leader_AndUnSuccess_DecremebntSentLength_And_ReplicateLogToFollower()
        {
            _ = UseNodeAsLeader();

            ResetCluster();

            var logResponse = new LogResponseMessage
            {
                Type = MessageType.LogResponse,
                Term = 11,
                NodeId = 1,
                Ack = 3,
                Success = false
            };

            var status = _sut.OnReceivedLogResponse(logResponse);

            status.SentLength[1].Should().Be(5);
            _cluster
                .Verify(m => m.SendMessage(1,
                                            It.Is<LogRequestMessage>(p =>
                                                p.Type == MessageType.LogRequest &&
                                                p.LeaderId == 42 &&
                                                p.LogTerm == 10 &&
                                                p.LogLength == 5)), Times.Once);
        }
    }
}

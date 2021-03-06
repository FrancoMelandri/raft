using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnBroadcastMessageAgentTests : BaseUseCases
    {
        [Test]
        public void WhenNodeIsNotLeader_ForwardToLeader()
        {
            var nodeConfig = new BaseNodeConfiguration
            {
                Id = 42,
            };

            var status = new Status
            {
                CurrentTerm = 11,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            status = _sut.OnInitialise(nodeConfig, status);
            // TODO: how to set a leader
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 10,
                Granted = false
            };

            var statusResult = _sut.OnBroadcastMessage(message);

            statusResult.Should().BeEquivalentTo(status);
            //_cluster
            //    .Verify(m => m.SendMessage(2, message));
        }

        [Test]
        public void WhenNodeIsLeader_AppendMessageToLog_UpdateStatus_ReplicateLog()
        {
            _ = UseNodeAsLeader();

            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };

            ResetCluster();

            var status = _sut.OnBroadcastMessage(message);

            status.Log.Should().HaveCount(7);
            status.Log[6].Message.Should().Be(message);
            status.Log[6].Term.Should().Be(11);

            status.AckedLength[42].Should().Be(7);
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

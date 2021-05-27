using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnBroadcastMessageAgentTests : BaseUseCases
    {
        [Test]
        public void WhenNodeIsNotLeader_ForwardToLeader()
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
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            // TODO: how to set a leader
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 10,
                Granted = false
            };

            var descriptorResult = _sut.OnBroadcastMessage(message);

            descriptorResult.Should().BeEquivalentTo(descriptor);
            //_cluster
            //    .Verify(m => m.SendMessage(2, message));
        }

        [Test]
        public void WhenNodeIsLeader_AppendMessageToLog_UpdateDescriptor_ReplicateLog()
        {
            _ = UseCase_NodeAsLeader();

            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 11,
                Granted = false
            };

            ResetCluster();

            var descriptor = _sut.OnBroadcastMessage(message);

            descriptor.Log.Should().HaveCount(7);
            descriptor.Log[6].Message.Should().Be(message);
            descriptor.Log[6].Term.Should().Be(11);

            descriptor.AckedLength[42].Should().Be(7);
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

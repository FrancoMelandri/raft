using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedLogRequestAgentTests : BaseUseCases
    {
        [Test]
        public void WhenReceivingTerm_IsGreaterThanTerm_UpdateTermAndVoteFor()
        {
            _ = UseNodeAsFollower();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 6,
                Entries = new LogEntry[] { },
                CommitLength = 5
            };
            var descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(15);
            descritpor.VotedFor.Should().Be(-1);
        }

        [Test]
        public void WhenReceivingTerm_IsLessOrEqualThanTerm_DontUpdateTermAndVoteFor()
        {
            _ = UseNodeAsFollower();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 5,
                LogTerm = 14,
                LogLength = 6,
                Entries = new LogEntry[] { },
                CommitLength = 5
            };
            var descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(10);
            descritpor.VotedFor.Should().Be(1);
        }

        [Test]
        public void WhenReceivingTerm_DescriptorLogLengthNotOk_UpdateTermAndVoteFor_AndNack()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 5,
                Entries = new LogEntry[] { },
                CommitLength = 2
            };
            var descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(15);
            descritpor.VotedFor.Should().Be(-1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.CurrentTerm == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Length == 0 &&
                                                    p.Ack == false)));
        }

        [Test]
        public void WhenReceivingTerm_DescriptorLogLength_TermNotOk_UpdateTermAndVoteFor_AndNack()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 3,
                Entries = new LogEntry[] { },
                CommitLength = 2
            };
            var descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(15);
            descritpor.VotedFor.Should().Be(-1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.CurrentTerm == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Length == 0 &&
                                                    p.Ack == false)));
        }

        [Test]
        public void WhenReceivingTerm_DescriptorLogLength_TermOk_UpdateTermAndVoteFor_AndAck()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 9,
                LogLength = 3,
                Entries = new LogEntry[] 
                        {
                            new LogEntry { Term = 9 },
                            new LogEntry { Term = 10 },
                            new LogEntry { Term = 11 } 
                        },
                CommitLength = 2
            };
            var descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(15);
            descritpor.VotedFor.Should().Be(-1);
            descritpor.CurrentRole.Should().Be(States.Follower);
            descritpor.CurrentLeader.Should().Be(1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.CurrentTerm == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Length == 0 &&
                                                    p.Ack == false)), Times.Never);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.CurrentTerm == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Length == 6 &&
                                                    p.Ack == true)), Times.Once);
        }
    }
}

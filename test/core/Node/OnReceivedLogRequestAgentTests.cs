using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedLogRequestAgentTests : BaseUseCases
    {
        [Test]
        public void WhenReceivingTerm_IsGreaterThanTerm_UpdateTermAndVoteFor()
        {
            _ = UseCase_NodeAsFollower();

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
            _ = UseCase_NodeAsFollower();

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
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.CurrentTerm == 10 &&
                                                    p.NodeId == 42 &&
                                                    p.Length == 0 &&
                                                    p.Ack == false)));
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedLogRequestAgentTests : BaseUseCases
    {
        [Test]
        public void WhenReceivingTerm_IsGreaterThanTerm_UpdateTermAndVoetFor()
        {
            var descritpor = UseCase_NodeAsFollower();

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
            descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(15);
            descritpor.VotedFor.Should().Be(-1);
        }

        [Test]
        public void WhenReceivingTerm_IsLessOrEqualThanTerm_DontUpdateTermAndVoetFor()
        {
            var descritpor = UseCase_NodeAsFollower();

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
            descritpor = _sut.OnReceivedLogRequest(logRequestMerssage);

            descritpor.CurrentTerm.Should().Be(10);
            descritpor.VotedFor.Should().Be(1);
        }
    }
}

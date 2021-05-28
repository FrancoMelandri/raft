using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
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

            var descriptor = _sut.OnReceivedLogResponse(logResponse);

            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.CurrentTerm.Should().Be(12);
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class AgentTests
    {
        [Test]
        public void Initialise_SetTheRightValues()
        {
            var agent = Agent.Initialise();

            agent.Descriptor.CurrentTerm.Should().Be(0);
            agent.Descriptor.VotedFor.Should().Be(-1);
            agent.Descriptor.Log.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.CommitLenght.Should().Be(0);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
            agent.Descriptor.CurrentLeader.Should().Be(-1);
            agent.Descriptor.VotesReceived.Should().NotBeNull();
            agent.Descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }
    }
}

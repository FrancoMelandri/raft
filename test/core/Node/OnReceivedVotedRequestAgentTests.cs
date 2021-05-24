using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedVotedRequestAgentTests
    {
        private Agent _sut;
        private Mock<ICluster> _cluster;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _sut = Agent.Create(_cluster.Object);
        }

        [Test]
        public void NodeLogTerm_GreaterThe_CandidateLog_RespondWithFalse()
        {            
        }
    }
}

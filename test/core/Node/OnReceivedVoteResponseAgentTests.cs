using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedVoteResponseAgentTests
    {
        private Agent _sut;
        private Mock<ICluster> _cluster;
        private Mock<IElection> _election;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _election = new Mock<IElection>();
            _sut = Agent.Create(_cluster.Object,
                                _election.Object);
        }
    }
}

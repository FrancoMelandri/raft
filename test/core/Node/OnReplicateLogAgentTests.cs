using Moq;
using NUnit.Framework;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReplicateLogAgentTests : BaseUseCases
    {
        [Test]
        public void WhenNodeIsFollwer_DontReplicateLog()
        {
            _ = UseCase_NodeAsFollower();
            _ = _sut.OnReplicateLog();

            _cluster
                .Verify(m => m.ReplicateLog(42, It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void WhenNodeIsLeader_ReplicateLog()
        {
            _ = UseCase_NodeAsLeader();

            ResetCluster();

            _ = _sut.OnReplicateLog();

            _cluster
                .Verify(m => m.ReplicateLog(42, 1), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 2), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 99), Times.Once);
            _cluster
                .Verify(m => m.ReplicateLog(42, 42), Times.Never);
        }
    }
}

using Moq;
using NUnit.Framework;
using RaftCore.Models;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReplicateLogAgentTests : BaseUseCases
    {
        [Test]
        public void WhenNodeIsFollwer_DontReplicateLog()
        {
            _ = UseNodeAsFollower();
            _ = _sut.OnReplicateLog();

            _cluster
                .Verify(m => m.SendMessage(It.IsAny<int>(),
                                            It.IsAny<LogRequestMessage>()), Times.Never);
        }

        [Test]
        public void WhenNodeIsLeader_ReplicateLog()
        {
            _ = UseNodeAsLeader();

            ResetCluster();

            _ = _sut.OnReplicateLog();

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

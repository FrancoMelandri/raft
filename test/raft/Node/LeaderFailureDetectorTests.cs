using Moq;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Cluster;
using System.Threading.Tasks;

namespace RaftTest.Raft
{
    [TestFixture]
    public class LeaderFailureDetectorTests
    {
        private LeaderFailureDetector _sut;
        private Mock<ILeaderFailureObserver> _leaderFailureObserver;

        [SetUp]
        public void SetUp()
        {
            var config = new LocalNodeConfiguration
            {
                LeaderFailureTimeout = 1000
            };
            _leaderFailureObserver = new Mock<ILeaderFailureObserver>();
            _sut = new LeaderFailureDetector(config);
        }

        [Test]
        public void WhenTimeOut_NotifyToObserver()
        {
            _ = _sut.Start(_leaderFailureObserver.Object);

            Task.Delay(2000).Wait();

            _ = _sut.Stop();

            _leaderFailureObserver
                .Verify(m => m.NotifyFailure(), Times.Once);
        }

        [Test]
        public void WhenStop_Beforetimeout_DontNotifyToObserver()
        {
            _ = _sut.Start(_leaderFailureObserver.Object);

            Task.Delay(500).Wait();

            _ = _sut.Stop();

            _leaderFailureObserver
                .Verify(m => m.NotifyFailure(), Times.Never);
        }

        [Test]
        public void WhenReset_Beforetimeout_DontNotifyToObserver()
        {
            _ = _sut.Start(_leaderFailureObserver.Object);

            Task.Delay(500).Wait();
            _ = _sut.Reset();

            Task.Delay(500).Wait();
            _ = _sut.Reset();

            Task.Delay(500).Wait();
            _ = _sut.Reset();

            Task.Delay(500).Wait();
            _ = _sut.Reset();

            Task.Delay(500).Wait();
            _ = _sut.Stop();

            _leaderFailureObserver
                .Verify(m => m.NotifyFailure(), Times.Never);
        }
    }
}

using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Adapters;
using RaftCore.Node;
using System.Threading.Tasks;
using TinyFp;

namespace RaftTest.Raft
{
    [TestFixture]
    public class ElectionTest
    {
        private LocalNodeConfiguration _config;
        private Election _sut;
        private Mock<IElectionObserver> _electionObserver;
        private Mock<ILogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _config = new LocalNodeConfiguration
            {
                ElectionTimeout = 500
            };
            _electionObserver = new Mock<IElectionObserver>();
            _logger = new Mock<ILogger>();
            _sut = new Election(_config,
                                _logger.Object);
        }

        [Test]
        public void Start_WhenElectioObserverIsSet_StartTheTimer_AndNotify()
        {
            _ = _sut.RegisterObserver(_electionObserver.Object);
            _ = _sut.Start();

            Task.Delay(750).Wait();

            _electionObserver
                .Verify(m => m.NotifyElectionTimeout(), Times.Once);
            _logger
                .Verify(m => m.Error(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Start_WhenElectioObserverIsSet_StartTheTimer_AndNotifyTwice()
        {
            _ = _sut.RegisterObserver(_electionObserver.Object);
            _ = _sut.Start();

            Task.Delay(1250).Wait();

            _electionObserver
                .Verify(m => m.NotifyElectionTimeout(), Times.Exactly(2));
            _logger
                .Verify(m => m.Error(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Start_WhenElectioObserverIsNotSet_StartTheTimer_AndDontNotify()
        {
            _ = _sut.Start();

            Task.Delay(750).Wait();

            _electionObserver
                .Verify(m => m.NotifyElectionTimeout(), Times.Never);
            _logger
                .Verify(m => m.Error("EL-0001: election-observer-not-registered"), Times.Once);
        }

        [Test]
        public void RegisterObserver_DoNothing()
            => _sut
                .RegisterObserver(_electionObserver.Object)
                .Should().Be(Unit.Default);

        [Test]
        public void Cancel_StopTheTimer()
        {
            _ = _sut.RegisterObserver(_electionObserver.Object);
            _ = _sut.Start();

            Task.Delay(100).Wait();
            _ = _sut.Stop();

            Task.Delay(600).Wait();
            _electionObserver
                .Verify(m => m.NotifyElectionTimeout(), Times.Never);
            _logger
                .Verify(m => m.Error(It.IsAny<string>()), Times.Never);
        }
    }
}

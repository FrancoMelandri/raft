using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Node;
using TinyFp;
using System.Threading.Tasks;

namespace RaftTest.Raft
{
    [TestFixture]
    public class LeaderTest
    {
        private Leader _sut;
        private Mock<IAgent> _agent;
        private LocalNodeConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _config = new LocalNodeConfiguration
            {
                LeaderReplicateLogInterval = 500
            };
            _agent = new Mock<IAgent>();
            _sut = new Leader(_config);
        }

        [Test]
        public void Start_CallingReplicateLogPeriodicaLLY()
        {
            _ = _sut.Start(_agent.Object);

            Task.Delay(1100).Wait();

            _agent
                .Verify(m => m.OnReplicateLog(), Times.Exactly(2));
        }

        [Test]
        public void Stop_StoppingTimer()
        {
            _ = _sut.Start(_agent.Object);
            Task.Delay(600).Wait();

            _ = _sut.Stop();
            Task.Delay(600).Wait();

            _agent
                .Verify(m => m.OnReplicateLog(), Times.Exactly(1));
        }
    }
}
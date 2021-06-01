using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Adapters;
using RaftCore.Models;
using RaftCore.Node;
using System.IO;

namespace RaftTest.Raft
{
    [TestFixture]
    public class RaftNodeTests
    {
        private LocalNode _sut;
        private LocalNodeConfiguration _nodeConfiguration;
        private Mock<IAgent> _agent;
        private Mock<IStatusRepository> _statusRepository;

        [SetUp]
        public void SetUp()
        {
            _nodeConfiguration = new LocalNodeConfiguration
            {
                Id = 1,
            };
            _agent = new Mock<IAgent>();
            _statusRepository = new Mock<IStatusRepository>();
            _sut = new LocalNode(_nodeConfiguration,
                                  _agent.Object,
                                  _statusRepository.Object);
        }

        [Test]
        public void ClusterNode_HasCorrectId()
            => _sut.Id.Should().Be(1);

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not-exists")]
        public void Initialise_WhenFileDoesntExist_OnInitialize(string fileName)
        {
            _nodeConfiguration.StatusFileName = fileName;

            _ = _sut.Initialise();

            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration), Times.Once);
            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration, It.IsAny<Status>()), Times.Never);
        }

        [Test]
        public void Initialise_WhenFileExist_OnInitialize()
        {
            _nodeConfiguration.StatusFileName = Path.Combine("resources", "status.json");

            _ = _sut.Initialise();

            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration), Times.Never);
            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration, 
                                            It.Is<Status>(p => 
                                                p.CurrentTerm == 1 &&
                                                p.VotedFor == 2 &&
                                                p.CommitLenght == 1 &&
                                                p.Log[0].Term == 1 &&
                                                p.Log[0].Message.Type == MessageType.VoteResponse)), Times.Once);
        }

        [Test]
        public void Deinitialise_SaveStatusToFile()
        {
            _nodeConfiguration.StatusFileName = Path.Combine("resources", "status.json");

            _ = _sut.Initialise();

            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration), Times.Never);
            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration, 
                                            It.Is<Status>(p => 
                                                p.CurrentTerm == 1 &&
                                                p.VotedFor == 2 &&
                                                p.CommitLenght == 1 &&
                                                p.Log[0].Term == 1 &&
                                                p.Log[0].Message.Type == MessageType.VoteResponse)), Times.Once);
        }
    }
}

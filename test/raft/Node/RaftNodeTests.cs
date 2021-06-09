using FluentAssertions;
using Moq;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Adapters;
using RaftCore.Models;
using RaftCore.Node;
using TinyFp;

namespace RaftTest.Raft
{
    [TestFixture]
    public class RaftNodeTests
    {
        private LocalNode _sut;
        private LocalNodeConfiguration _nodeConfiguration;
        private Mock<IAgent> _agent;
        private Mock<IStatusRepository> _statusRepository;
        private Mock<IMessageListener> _messageListener;
        private Mock<ILeaderFailureDetector> _leaderFailure;

        [SetUp]
        public void SetUp()
        {
            _nodeConfiguration = new LocalNodeConfiguration
            {
                Id = 1,
            };
            _agent = new Mock<IAgent>();
            _statusRepository = new Mock<IStatusRepository>();
            _messageListener = new Mock<IMessageListener>();
            _leaderFailure = new Mock<ILeaderFailureDetector>();
            _sut = new LocalNode(_nodeConfiguration,
                                  _agent.Object,
                                  _statusRepository.Object,
                                  _messageListener.Object,
                                  _leaderFailure.Object);
        }

        [Test]
        public void ClusterNode_HasCorrectId()
            => _sut.Id.Should().Be(1);

        [Test]
        public void Initialise_WhenFileDoesntExist_OnInitialize()
        {
            _statusRepository
                .Setup(m => m.Load())
                .Returns(Option<Status>.None());

            _ = _sut.Initialise();

            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration), Times.Once);
            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration, It.IsAny<Status>()), Times.Never);
            _messageListener
                .Verify(m => m.Start(_sut), Times.Once);
            _leaderFailure
                .Verify(m => m.Start(_sut), Times.Once);
        }

        [Test]
        public void Initialise_WhenFileExist_OnInitialize()
        {
            var status = new Status
            {
                CurrentTerm = 1,
                VotedFor = 2,
                CommitLenght = 1,
                Log = new LogEntry[] 
                {
                    new LogEntry
                    {
                        Term = 1,
                        Message = new VoteResponseMessage
                        {
                            Type = MessageType.VoteResponse
                        }
                    }
                }
            };

            _statusRepository
                .Setup(m => m.Load())
                .Returns(Option<Status>.Some(status));

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
            _messageListener
                .Verify(m => m.Start(_sut), Times.Once);
            _leaderFailure
                .Verify(m => m.Start(_sut), Times.Once);
        }

        [Test]
        public void Deinitialise_SaveStatusToFile()
        {
            var status = new Status();
            _agent
                .Setup(m => m.CurrentStatus())
                .Returns(status);

            _ = _sut.Deinitialise();

            _agent
                .Verify(m => m.CurrentStatus(), Times.Once);
            _statusRepository
                .Verify(m => m.Save(status), Times.Once);
            _messageListener
                .Verify(m => m.Stop(), Times.Once);
            _leaderFailure
                .Verify(m => m.Stop(), Times.Once);
        }

        [TestCase(MessageType.None)]
        [TestCase(MessageType.LogResponse)]
        [TestCase(MessageType.VoteRequest)]
        [TestCase(MessageType.VoteResponse)]
        public void NotifyMessage_ReturnOk(MessageType type)
        {
            var message = new Message
            {
                Type = type
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);
        }

        [Test]
        public void NotifyFailure_CallAgentLeaderHasFailed()
        {
            _ = _sut.NotifyFLeaderailure();

            _agent
                .Verify(m => m.OnLeaderHasFailed(), Times.Once);
        }

        [Test]
        public void NotifyElectionTimeout_CallElectionTimeout()
        {
            _ = _sut.NotifyElectionTimeout();

            _agent
                .Verify(m => m.OnElectionTimeOut(), Times.Once);
        }

        [Test]
        public void NotifyMessage_WhenLogRequest_CallAgentReceivedLogRequest_And_ResetLeaderDetector()
        {
            var message = new LogRequestMessage
            {
                Type = MessageType.LogRequest
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedLogRequest(message), Times.Once);
            _leaderFailure
                .Verify(m => m.Reset(), Times.Once);
        }

        [Test]
        public void NotifyMessage_WhenNotLogRequest_DontCallAgentReceivedLogRequest_And_DontResetLeaderDetector()
        {
            var message = new Message
            {
                Type = MessageType.LogRequest
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedLogRequest(It.IsAny<LogRequestMessage>()), Times.Never);
            _leaderFailure
                .Verify(m => m.Reset(), Times.Never);
        }

        [Test]
        public void NotifyMessage_WhenVoteResponse_CallAgentReceivedVoteResponse()
        {
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedVoteResponse(message), Times.Once);
        }

        [Test]
        public void NotifyMessage_WhenNotVoteResponse_DontCallAgentReceivedVoteResponse()
        {
            var message = new Message
            {
                Type = MessageType.VoteResponse
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedVoteResponse(It.IsAny<VoteResponseMessage>()), Times.Never);
        }

        [Test]
        public void NotifyMessage_WhenLogResponse_CallAgentReceivedLogReponse()
        {
            var message = new LogResponseMessage
            {
                Type = MessageType.LogResponse
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedLogResponse(message), Times.Once);
        }

        [Test]
        public void NotifyMessage_WhenNotLogResponse_DontCallAgentReceivedLogReponse()
        {
            var message = new Message
            {
                Type = MessageType.LogResponse
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedLogResponse(It.IsAny<LogResponseMessage>()), Times.Never);
        }

        [Test]
        public void NotifyMessage_WhenVoteRequest_CallAgentReceivedVoteRequest()
        {
            var message = new VoteRequestMessage
            {
                Type = MessageType.VoteRequest
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedVoteRequest(message), Times.Once);
        }

        [Test]
        public void NotifyMessage_WhenNotVoteRequest_DontCallAgentReceivedVoteRequest()
        {
            var message = new Message
            {
                Type = MessageType.VoteRequest
            };
            _sut.NotifyMessage(message)
                .Should().Be(Unit.Default);

            _agent
                .Verify(m => m.OnReceivedVoteRequest(It.IsAny<VoteRequestMessage>()), Times.Never);
        }
    }
}

﻿using FluentAssertions;
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
            _sut = new LocalNode(_nodeConfiguration,
                                  _agent.Object,
                                  _statusRepository.Object,
                                  _messageListener.Object);
        }

        [Test]
        public void ClusterNode_HasCorrectId()
            => _sut.Id.Should().Be(1);

        [Test]
        public void Initialise_WhenFileDoesntExist_OnInitialize()
        {
            _statusRepository
                .Setup(m => m.LoadStatus())
                .Returns(Option<Status>.None());

            _ = _sut.Initialise();

            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration), Times.Once);
            _agent
                .Verify(m => m.OnInitialise(_nodeConfiguration, It.IsAny<Status>()), Times.Never);
            _messageListener
                .Verify(m => m.Start(), Times.Once);
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
                .Setup(m => m.LoadStatus())
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
                .Verify(m => m.Start(), Times.Once);
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
                .Verify(m => m.SaveStatus(status), Times.Once);
            _messageListener
                .Verify(m => m.Stop(), Times.Once);
        }
    }
}

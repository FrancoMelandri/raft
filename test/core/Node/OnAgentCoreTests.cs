using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnAgentCoreTests : BaseUseCases
    {
        [Test]
        public void Quorum_ShouldBeRight()
            => RaftCore
                .Utils
                .GetQuorum(_cluster.Object)
                .Should().Be(2);

        [Test]
        public void CommitLogEntries_WhenReadyIsEmpty_DoNothing()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5

                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 6,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int>
                {
                    { 1, 0 },
                    { 2, 0 },
                    { 3, 0 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(6);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public void CommitLogEntries_WhenReadyIsWrong_DueNoOneNodeHaveSussificentAcks_DoNothing()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5

                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 6,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int> 
                {
                    { 1, 6 },
                    { 2, 6 },
                    { 3, 6 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(6);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public void CommitLogEntries_WhenReadyIsWrong_WhenAtOneNode_ReachedAcks_DoNothing()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5
                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 5,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int>
                {
                    { 1, 6 },
                    { 2, 5 },
                    { 3, 0 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(5);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public void CommitLogEntries_WhenAtLeastOneNode_ReachedAcks_NotifyApplication()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5
                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 5,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int> 
                {
                    { 1, 6 },
                    { 2, 7 },
                    { 3, 0 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(6);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Once);
        }        
        
        [Test]
        public void CommitLogEntries_WhenMoreNodes_ReachedAcks_NotifyApplication()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5
                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 5,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int> 
                {
                    { 1, 7 },
                    { 2, 7 },
                    { 3, 0 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(7);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Exactly(2));
        }

        [Test]
        public void CommitLogEntries_WhenAllNodes_ReachedAcks_NotifyApplication()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5
                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 5,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int>
                {
                    { 1, 7 },
                    { 2, 7 },
                    { 3, 7 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(7);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Exactly(2));
        }

        [Test]
        public void CommitLogEntries_WhenMoreNodes_ReachedAcks_ButWrongCommitLength_DoNothign()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 2 }, // 4
                        new LogEntry { Message = new Message(), Term = 2 }, // 5
                        new LogEntry { Message = new Message(), Term = 2 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 7,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int> 
                {
                    { 1, 7 },
                    { 2, 7 },
                    { 3, 0 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(7);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public void CommitLogEntries_WhenAllNodes_ReachedAcks_ButTermIsWrong_DoNothing()
        {
            var descriptor = new Status
            {
                Log = new LogEntry[] {
                        new LogEntry { Message = new Message(), Term = 0 }, // 0
                        new LogEntry { Message = new Message(), Term = 0 }, // 1
                        new LogEntry { Message = new Message(), Term = 1 }, // 2
                        new LogEntry { Message = new Message(), Term = 1 }, // 3
                        new LogEntry { Message = new Message(), Term = 1 }, // 4
                        new LogEntry { Message = new Message(), Term = 1 }, // 5
                        new LogEntry { Message = new Message(), Term = 1 }, // 6
                        new LogEntry { Message = new Message(), Term = 2 }, // 7
                        new LogEntry { Message = new Message(), Term = 2 }, // 8
                        new LogEntry { Message = new Message(), Term = 2 }, // 9
                },
                CommitLenght = 5,
                CurrentRole = States.Leader,
                CurrentTerm = 2,
                AckedLength = new Dictionary<int, int>
                {
                    { 1, 7 },
                    { 2, 7 },
                    { 3, 7 },
                }
            };

            descriptor = _sut.CommitLogEntries(descriptor);

            descriptor.CommitLenght.Should().Be(5);
            _application
                .Verify(m => m.NotifyMessage(It.IsAny<Message>()), Times.Never);
        }
    }
}

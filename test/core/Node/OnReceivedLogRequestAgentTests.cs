using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnReceivedLogRequestAgentTests : BaseUseCases
    {
        [Test]
        public void WhenReceivingTerm_IsGreaterThanTerm_UpdateTermAndVoteFor()
        {
            _ = UseNodeAsFollower();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 6,
                Entries = new LogEntry[] { },
                CommitLength = 5
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);

            _logger
                .Verify(m => m.Error("LR-0002: message-length-is-not-ok"));
        }

        [Test]
        public void WhenReceivingTerm_IsLessOrEqualThanTerm_DontUpdateTermAndVoteFor()
        {
            _ = UseNodeAsFollower();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 5,
                LogTerm = 14,
                LogLength = 6,
                Entries = new LogEntry[] { },
                CommitLength = 5
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(10);
            status.VotedFor.Should().Be(1);

            _logger
                .Verify(m => m.Error("LR-0001: term-is-not-greater"));
        }

        [Test]
        public void WhenReceivingTerm_StatusLogLengthNotOk_UpdateTermAndVoteFor_AndNack()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 5,
                Entries = new LogEntry[] { },
                CommitLength = 2
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)));
        }

        [Test]
        public void WhenReceivingTerm_StatusLogLength_TermNotOk_UpdateTermAndVoteFor_AndNack()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 14,
                LogLength = 3,
                Entries = new LogEntry[] { },
                CommitLength = 2
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)));
            _logger
                .Verify(m => m.Error("LR-0003: message-term-is-not-ok"));
        }

        [Test]
        public void WhenReceivingTerm_StatusLogLength_TermOk_UpdateTermAndVoteFor_AndAck()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 8,
                LogLength = 3,
                Entries = new LogEntry[] 
                        {
                            new LogEntry { Term = 9 },
                            new LogEntry { Term = 10 },
                            new LogEntry { Term = 11 } 
                        },
                CommitLength = 2
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(1);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)), Times.Never);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 6 &&
                                                    p.Success == true)), Times.Once);
        }

        [Test]
        public void WhenLogRequestOk_And_MistmatchInLogLength_TruncateTheLog()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 8,
                LogLength = 3,
                Entries = new LogEntry[] 
                        {
                            new LogEntry { Term = 10 },
                            new LogEntry { Term = 11 },
                            new LogEntry { Term = 12 } 
                        },
                CommitLength = 2
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(1);
            status.Log.Should().BeEquivalentTo(new LogEntry[] {
                        new LogEntry { Term = 6 },
                        new LogEntry { Term = 7 },
                        new LogEntry { Term = 8 },
                        new LogEntry { Term = 10 },
                        new LogEntry { Term = 11 },
                        new LogEntry { Term = 12 } });
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)), Times.Never);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 6 &&
                                                    p.Success == true)), Times.Once);

            _logger
                .Verify(m => m.Information("Log was trucated"));
        }

        [Test]
        public void WhenLogRequestOk_And_NewEntriesInMessage_AppendToLog()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 11,
                LogLength = 6,
                Entries = new LogEntry[]
                        {
                            new LogEntry { Term = 12 },
                            new LogEntry { Term = 13 },
                            new LogEntry { Term = 14 }
                        },
                CommitLength = 2
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(1);
            status.Log.Should().BeEquivalentTo(new LogEntry[] {
                        new LogEntry { Term = 6 },
                        new LogEntry { Term = 7 },
                        new LogEntry { Term = 8 },
                        new LogEntry { Term = 9 },
                        new LogEntry { Term = 10 },
                        new LogEntry { Term = 11 },
                        new LogEntry { Term = 12 },
                        new LogEntry { Term = 13 },
                        new LogEntry { Term = 14 }});
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)), Times.Never);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 9 &&
                                                    p.Success == true)), Times.Once);
            _logger
                .Verify(m => m.Information("Append new entries to log"));
        }

        [Test]
        public void WhenAll_IsOk_NotifyApplication()
        {
            _ = UseNodeAsLeader();

            var logRequestMerssage = new LogRequestMessage
            {
                Type = MessageType.LogRequest,
                LeaderId = 1,
                Term = 15,
                LogTerm = 11,
                LogLength = 6,
                Entries = new LogEntry[]
                        {
                            new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 12 },
                            new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 13 },
                            new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 14 }
                        },
                CommitLength = 9
            };
            var status = _sut.OnReceivedLogRequest(logRequestMerssage);

            status.CurrentTerm.Should().Be(15);
            status.VotedFor.Should().Be(-1);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(1);
            status.Log.Should().BeEquivalentTo(new LogEntry[] {
                        new LogEntry { Term = 6 },
                        new LogEntry { Term = 7 },
                        new LogEntry { Term = 8 },
                        new LogEntry { Term = 9 },
                        new LogEntry { Term = 10 },
                        new LogEntry { Term = 11 },
                        new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 12 },
                        new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 13 },
                        new LogEntry { Message = new Message{ Type = MessageType.None }, Term = 14 }});
            status.CommitLenght.Should().Be(9);
            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 0 &&
                                                    p.Success == false)), Times.Never);

            _cluster
                .Verify(m => m.SendMessage(1, It.Is<LogResponseMessage>(p =>
                                                    p.Term == 15 &&
                                                    p.NodeId == 42 &&
                                                    p.Ack == 9 &&
                                                    p.Success == true)), Times.Once);
            _application
                .Verify(m => m.NotifyMessage(It.Is<Message>(p => p.Type == MessageType.None)), Times.Exactly(3));

            _logger
                .Verify(m => m.Information("Notify to application"));
        }
    }
}

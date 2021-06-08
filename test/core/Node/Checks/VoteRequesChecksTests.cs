using FluentAssertions;
using NUnit.Framework;
using RaftCore;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftTest.Core.Checks
{

    [TestFixture]
    public class VoteRequesChecksTests
    {
        [Test]
        public void ValidateLog_WhenLastTerm_GT_LastLogTerm_ReturnStatus()
        {
            var message = new VoteRequestMessage
            {
                LastTerm = 5
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry{ Term = 4 }
                }
            };
            VoteRequesChecks
                .ValidateLog(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void ValidateLog_WhenLastTerm_EQ_LastLogTerm_And_LogLength_GT_StatusLogLength_ReturnStatus()
        {
            var message = new VoteRequestMessage
            {
                LastTerm = 5,
                LogLength = 2
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry{ Term = 5 }
                }
            };
            var result = VoteRequesChecks.ValidateLog(message, status);

            result.IsRight.Should().BeTrue();
        }

        [Test]
        public void ValidateLog_WhenLastTerm_GT_LastLogTerm_ReturnSError()
        {
            var message = new VoteRequestMessage
            {
                LastTerm = 3
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry{ Term = 4 }
                }
            };
            var result = VoteRequesChecks.ValidateLog(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VL-0001", "last-term-and-log-are-wrong")));
        }

        [Test]
        public void ValidateLog_WhenLastTerm_EQ_LastLogTerm_And_LogLength_EQ_StatusLogLength_ReturnError()
        {
            var message = new VoteRequestMessage
            {
                LastTerm = 5,
                LogLength = 1
            };
            var status = new Status
            {
                Log = new LogEntry[]
                {
                    new LogEntry{ Term = 5 },
                    new LogEntry{ Term = 5 }
                }
            };
            var result = VoteRequesChecks.ValidateLog(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VL-0001", "last-term-and-log-are-wrong")));
        }

        [Test]
        public void ValidateTerm_WhenCurrentTerm_GT_StatusCurrentTerm_ReturnStatus()
        {
            var message = new VoteRequestMessage
            {
                CurrentTerm = 5
            };
            var status = new Status
            {
                CurrentTerm = 4
            };
            VoteRequesChecks
                .ValidateTerm(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void ValidateTerm_WhenCurrentTerm_EQ_StatusCurrentTerm_And_StatusVoteFor_NEQ_MessageNodeId_ReturnStatus()
        {
            var message = new VoteRequestMessage
            {
                CurrentTerm = 4,
                NodeId = 1
            };
            var status = new Status
            {
                CurrentTerm = 4,
                VotedFor = 2
            };
            VoteRequesChecks
                .ValidateTerm(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void ValidateTerm_WhenCurrentTerm_EQ_StatusCurrentTerm_And_StatusVoteFor_EQ_MessageNodeId_ReturnError()
        {
            var message = new VoteRequestMessage
            {
                CurrentTerm = 4,
                NodeId = 1
            };
            var status = new Status
            {
                CurrentTerm = 4,
                VotedFor = 1
            };
            var result = VoteRequesChecks.ValidateTerm(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VL-0002", "current-term-and-votedfor-are-wrong")));
        }
    }
}

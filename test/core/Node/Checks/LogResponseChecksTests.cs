using FluentAssertions;
using NUnit.Framework;
using RaftCore;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core.Checks
{

    [TestFixture]
    public class LogResponseChecksTests
    {
        [Test]
        public void IsTermLessOrEqualGreater_WhenTerm_GT_CurrentTerm_ReturnError()
        {
            var message = new LogResponseMessage
            {
                Term = 4
            };
            var status = new Status
            {
                CurrentTerm = 3
            };
            var result = LogResponseChecks
                .IsTermLessOrEqualGreater(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("LR-0001", "term-is-not-greater")));
        }

        [TestCase(5)]
        [TestCase(6)]
        public void IsTermLessOrEqualGreater_WhenTerm_LE_CurrentTerm_ReturnOk(int current)
        {
            var message = new LogResponseMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = current
            };
            LogResponseChecks
                .IsTermLessOrEqualGreater(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsTermEqual_WhenTerm_EQ_CurrentTerm_ReturnStatus()
        {
            var message = new LogResponseMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = 5
            };
            LogResponseChecks
                .IsTermEqual(message, status)
                .IsRight.Should().BeTrue();
        }

        [TestCase(4)]
        [TestCase(6)]
        public void IsTermEqual_WhenTerm_NEQ_CurrentTerm_ReturnError(int current)
        {
            var message = new LogResponseMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = current
            };
            var result = LogResponseChecks.IsTermEqual(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("LR-0007", "term-is-not-equal")));
        }

        [Test]
        public void IsSentLengthGreaterThanZero_GT_Zero_ReturnStatus()
        {
            var message = new LogResponseMessage
            {
                NodeId = 1
            };
            var status = new Status
            {
                SentLength = new Dictionary<int, int>
                {
                    { 1, 1 }
                }
            };
            LogResponseChecks
                .IsSentLengthGreaterThanZero(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsSentLengthGreaterThanZero_EQ_Zero_ReturnError()
        {
            var message = new LogResponseMessage
            {
                NodeId = 1
            };
            var status = new Status
            {
                SentLength = new Dictionary<int, int>
                {
                    { 1, 0 }
                }
            };
            var result = LogResponseChecks.IsSentLengthGreaterThanZero(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("LR-0010", "sent-length-is-wrong")));
        }

        [Test]
        public void IsSentLengthGreaterThanZero_NotExists_ReturnError()
        {
            var message = new LogResponseMessage
            {
                NodeId = 1
            };
            var status = new Status
            {
                SentLength = new Dictionary<int, int>
                {
                    { 2, 1 }
                }
            };
            var result = LogResponseChecks.IsSentLengthGreaterThanZero(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("LR-0010", "sent-length-is-wrong")));
        }

        [Test]
        public void IsSuccessLogReponse_True_ReturnStatus()
        {
            var message = new LogResponseMessage
            {
                Success = true
            };
            var status = new Status
            {
            };
            LogResponseChecks
                .IsSuccessLogReponse(message, status)
                .IsRight.Should().BeTrue();
        }

        [Test]
        public void IsSuccessLogReponse_False_ReturnError()
        {
            var message = new LogResponseMessage
            {
                Success = false
            };
            var status = new Status
            {
            };
            var result = LogResponseChecks.IsSuccessLogReponse(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("LR-0009", "log-response-not-success")));
        }
    }
}

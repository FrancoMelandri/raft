using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core.Checks
{
    [TestFixture]
    public class LogResponseChecksTests
    {
        [Test]
        public void IsTermGreater_WhenTerm_GT_CurrentTerm_ReturnStatus()
        {
            var message = new LogResponseMessage
            {
                Term = 5
            };
            var status = new Status
            {
                CurrentTerm = 4
            };
            LogResponseChecks
                .IsTermGreater(message, status)
                .IsRight.Should().BeTrue();
        }

        [TestCase(5)]
        [TestCase(6)]
        public void IsTermGreater_WhenTerm_LE_CurrentTerm_ReturnError(int current)
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
                .IsTermGreater(message, status)
                .IsLeft.Should().BeTrue();
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
            LogResponseChecks
                .IsTermEqual(message, status)
                .IsLeft.Should().BeTrue();
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
            LogResponseChecks
                .IsSentLengthGreaterThanZero(message, status)
                .IsLeft.Should().BeTrue();
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
            LogResponseChecks
                .IsSentLengthGreaterThanZero(message, status)
                .IsLeft.Should().BeTrue();
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
            LogResponseChecks
                .IsSuccessLogReponse(message, status)
                .IsLeft.Should().BeTrue();
        }
    }
}

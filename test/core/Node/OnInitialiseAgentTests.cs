using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    [TestFixture]
    public class OnInitialiseAgentTests : BaseUseCases
    {
        [Test]
        public void WhenNoDescriptor_FirstInitialisation_SetTheRightValues()
        {
            var nodeConfig = new LocalNodeConfiguration
            {
                Id = 42
            };
            var status = _sut.OnInitialise(nodeConfig);
            
            status.CurrentTerm.Should().Be(0);
            status.VotedFor.Should().Be(-1);
            status.Log.Should().BeEquivalentTo(new LogEntry[] { });
            status.CommitLenght.Should().Be(0);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(-1);
            status.VotesReceived.Should().NotBeNull();
            status.SentLength.Should().BeEquivalentTo(new object[] { });
            status.AckedLength.Should().BeEquivalentTo(new object[] { });
        }

        [Test]
        public void CurrentStatus_IsCorrect()
        {
            var nodeConfig = new LocalNodeConfiguration
            {
                Id = 42
            };
            _ = _sut.OnInitialise(nodeConfig);
            var status = _sut.CurrentStatus();

            status.CurrentTerm.Should().Be(0);
            status.VotedFor.Should().Be(-1);
            status.Log.Should().BeEquivalentTo(new LogEntry[] { });
            status.CommitLenght.Should().Be(0);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(-1);
            status.VotesReceived.Should().NotBeNull();
            status.SentLength.Should().BeEquivalentTo(new object[] { });
            status.AckedLength.Should().BeEquivalentTo(new object[] { });

        }

        [Test]
        public void WhenDescriptor_RecoverFromCrash_SetTheRightValues()
        {
            var nodeConfig = new LocalNodeConfiguration
            {
                Id = 42
            };

            var status = new Status
            {
                CurrentTerm = 42,
                VotedFor = 42,
                Log = new LogEntry[] { new LogEntry() },
                CommitLenght = 42,
                CurrentRole = States.Leader,
                CurrentLeader = 42,
                VotesReceived = null,
                SentLength = new Dictionary<int, int> { {1, 1} },
                AckedLength = new Dictionary<int, int> { {1, 1} }
            };

            status = _sut.OnInitialise(nodeConfig, status);
            
            status.CurrentTerm.Should().Be(42);
            status.VotedFor.Should().Be(42);
            status.Log.Length.Should().Be(1);
            status.CommitLenght.Should().Be(42);
            status.CurrentRole.Should().Be(States.Follower);
            status.CurrentLeader.Should().Be(-1);
            status.VotesReceived.Should().NotBeNull();
            status.SentLength.Should().BeEquivalentTo(new Dictionary<int, int>());
            status.AckedLength.Should().BeEquivalentTo(new Dictionary<int, int>());
        }
    }
}

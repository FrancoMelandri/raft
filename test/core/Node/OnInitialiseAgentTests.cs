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
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };
            var descriptor = _sut.OnInitialise(nodeConfig);
            
            descriptor.CurrentTerm.Should().Be(0);
            descriptor.VotedFor.Should().Be(-1);
            descriptor.Log.Should().BeEquivalentTo(new LogEntry[] { });
            descriptor.CommitLenght.Should().Be(0);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.CurrentLeader.Should().Be(-1);
            descriptor.VotesReceived.Should().NotBeNull();
            descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }


        [Test]
        public void WhenDescriptor_RecoverFromCrash_SetTheRightValues()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
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

            descriptor = _sut.OnInitialise(nodeConfig, descriptor);
            
            descriptor.CurrentTerm.Should().Be(42);
            descriptor.VotedFor.Should().Be(42);
            descriptor.Log.Length.Should().Be(1);
            descriptor.CommitLenght.Should().Be(42);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.CurrentLeader.Should().Be(-1);
            descriptor.VotesReceived.Should().NotBeNull();
            descriptor.SentLength.Should().BeEquivalentTo(new Dictionary<int, int>());
            descriptor.AckedLength.Should().BeEquivalentTo(new Dictionary<int, int>());
        }
    }
}

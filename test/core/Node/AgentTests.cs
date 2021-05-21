﻿using FluentAssertions;
using NUnit.Framework;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class AgentTests
    {
        [Test]
        public void OnInitialise_SetTheRightValues()
        {
            var agent = Agent.OnInitialise(42);

            agent.NodeId.Should().Be(42);
            agent.Descriptor.CurrentTerm.Should().Be(0);
            agent.Descriptor.VotedFor.Should().Be(-1);
            agent.Descriptor.Log.Should().BeEquivalentTo(new LogEntry[] { });
            agent.Descriptor.CommitLenght.Should().Be(0);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
            agent.Descriptor.CurrentLeader.Should().Be(-1);
            agent.Descriptor.VotesReceived.Should().NotBeNull();
            agent.Descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }

        [Test]
        public void OnRecoverFromCrash_SetTheRightValues()
        {
            var descriptor = new Descriptor
            {
                CurrentTerm = 42,
                VotedFor = 42,
                Log = new LogEntry[] { new LogEntry() },
                CommitLenght = 42,
                CurrentRole = States.Leader,
                CurrentLeader = 42,
                VotesReceived = null,
                SentLength = new object[] { new object() },
                AckedLength = new object[] { new object() }
            };

            var agent = Agent.OnRecoverFromCrash(42, descriptor);

            agent.NodeId.Should().Be(42);
            agent.Descriptor.CurrentTerm.Should().Be(42);
            agent.Descriptor.VotedFor.Should().Be(42);
            agent.Descriptor.Log.Length.Should().Be(1);
            agent.Descriptor.CommitLenght.Should().Be(42);
            agent.Descriptor.CurrentRole.Should().Be(States.Follower);
            agent.Descriptor.CurrentLeader.Should().Be(-1);
            agent.Descriptor.VotesReceived.Should().NotBeNull();
            agent.Descriptor.SentLength.Should().BeEquivalentTo(new object[] { });
            agent.Descriptor.AckedLength.Should().BeEquivalentTo(new object[] { });
        }
    }
}

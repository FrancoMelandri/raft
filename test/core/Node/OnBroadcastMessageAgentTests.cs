using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnBroadcastMessageAgentTests
    {
        private Agent _sut;
        private Mock<ICluster> _cluster;
        private Mock<IElection> _election;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _election = new Mock<IElection>();
            _sut = Agent.Create(_cluster.Object,
                                _election.Object);
        }

        [Test]
        public void WhenNodeIsNotLeader_ForwardToLeader()
        {
            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 11,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Follower,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            descriptor = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            // TODO: how to set a leader
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 99,
                CurrentTerm = 10,
                Granted = false
            };

            var descriptorResult = _sut.OnBroadcastMessage(message);

            descriptorResult.Should().BeEquivalentTo(descriptor);
            //_cluster
            //    .Verify(m => m.SendMessage(2, message));
        }
    }
}

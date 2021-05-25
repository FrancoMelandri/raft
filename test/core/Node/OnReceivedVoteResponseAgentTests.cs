using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftCoreTest.Node
{
    [TestFixture]
    public class OnReceivedVoteResponseAgentTests
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
        public void CurrentRole_Not_Candidate_And_Term_LessOrEqual_CurrentTerm_DoNothing()
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
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new object[] { },
                AckedLength = new object[] { }
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteRequest,
                NodeId = 99,
                CurrentTerm = 12,
                Granted = false
            };
            descriptor = _sut.OnReceivedVoteResponse(message);

            descriptor.CurrentTerm.Should().Be(11);
            descriptor.CurrentRole.Should().Be(States.Follower);
            descriptor.VotedFor.Should().Be(1);
            descriptor.SentLength.Should().BeEmpty();
            descriptor.AckedLength.Should().BeEmpty();
            descriptor.VotesReceived.Should().BeEmpty();
        }
    }
}

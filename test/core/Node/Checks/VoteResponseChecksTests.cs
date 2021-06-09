using FluentAssertions;
using Moq;
using NUnit.Framework;
using RaftCore;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;

namespace RaftTest.Core
{
    [TestFixture]
    public class VoteResponseChecksTests
    {
        [Test]
        public void ValidateVoteGrant_WhenLCurrentRoleIsCandidate_And_CurrentTermIsOk_And_Granted_ReturnStatus()
        {
            var message = new VoteResponseMessage
            {
                Granted = true,
                CurrentTerm = 1
            };
            var status = new Status
            {
                CurrentRole = States.Candidate,
                CurrentTerm = 1
            };
            VoteResponseChecks
                .ValidateVoteGrant(message, status)
                .IsRight.Should().BeTrue();
        }

        [TestCase(States.Follower)]
        [TestCase(States.Leader)]
        public void ValidateVoteGrant_WhenLCurrentRoleIsNotCandidate_ReturnError(States state)
        {
            var message = new VoteResponseMessage
            {
                Granted = true,
                CurrentTerm = 1
            };
            var status = new Status
            {
                CurrentRole = state,
                CurrentTerm = 1
            };
            var result = VoteResponseChecks.ValidateVoteGrant(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0001", "vote-is-not-valid")));
        }

        [Test]
        public void ValidateVoteGrant_WhenLCurrentRoleIsCandidate_But_CurrentTerm_IsDifferent_ReturnError()
        {
            var message = new VoteResponseMessage
            {
                Granted = true,
                CurrentTerm = 2
            };
            var status = new Status
            {
                CurrentRole = States.Candidate,
                CurrentTerm = 1
            };
            var result = VoteResponseChecks.ValidateVoteGrant(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0001", "vote-is-not-valid")));
        }

        [Test]
        public void ValidateVoteGrant_WhenLCurrentRoleIsCandidate_And_CurrentTerm_IsEqual_But_NotGranted_ReturnError()
        {
            var message = new VoteResponseMessage
            {
                Granted = false,
                CurrentTerm = 1
            };
            var status = new Status
            {
                CurrentRole = States.Candidate,
                CurrentTerm = 1
            };
            var result = VoteResponseChecks.ValidateVoteGrant(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0001", "vote-is-not-valid")));
        }

        [Test]
        public void ValidateTerm_WhenMessageTerm_GT_CurrentTerm_ReturnStatus()
        {
            var message = new VoteResponseMessage
            {
                CurrentTerm = 3
            };
            var status = new Status
            {
                CurrentTerm = 1
            };
            VoteResponseChecks
                .ValidateTerm(message, status)
                .IsRight.Should().BeTrue();
        }

        [TestCase(3)]
        [TestCase(2)]
        public void ValidateTerm_WhenMessageTerm_LE_CurrentTerm_ReturnError(int term)
        {
            var message = new VoteResponseMessage
            {
                CurrentTerm = term
            };
            var status = new Status
            {
                CurrentTerm = 3
            };
            var result = VoteResponseChecks.ValidateTerm(message, status);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0002", "term-is-not-valid")));
        }

        [Test]
        public void ValidateVotesQuorum_WhenVotesReceived_IsNull_Returnerror()
        {
            var cluster = new Mock<ICluster>();

            var status = new Status
            {
                VotesReceived = null
            };
            var result = VoteResponseChecks.ValidateVotesQuorum(status, cluster.Object);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0003", "quorum-not-reached")));
        }

        [Test]
        public void ValidateVotesQuorum_WhenVotesReceived_isZero_Returnerror()
        {
            var cluster = new Mock<ICluster>();

            var status = new Status
            {
                VotesReceived = new int[] { }
            };
            var result = VoteResponseChecks.ValidateVotesQuorum(status, cluster.Object);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0003", "quorum-not-reached")));
        }

        [Test]
        public void ValidateVotesQuorum_WhenVotesReceived_LE_Quorum_Returnerror()
        {
            var node1 = new Mock<IClusterNode>();
            var node2 = new Mock<IClusterNode>();
            var node3 = new Mock<IClusterNode>();
            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.Nodes)
                .Returns(new IClusterNode[] { node1.Object, node2.Object, node3.Object });

            var status = new Status
            {
                VotesReceived = new int[] { 1, 2 }
            };
            var result = VoteResponseChecks.ValidateVotesQuorum(status, cluster.Object);

            result.IsLeft.Should().BeTrue();
            result.OnLeft(_ => _.Should().BeEquivalentTo(new Error("VR-0003", "quorum-not-reached")));
        }

        [Test]
        public void ValidateVotesQuorum_WhenVotesReceived_GT_Quorum_Returnerror()
        {
            var node1 = new Mock<IClusterNode>();
            var node2 = new Mock<IClusterNode>();
            var node3 = new Mock<IClusterNode>();
            var cluster = new Mock<ICluster>();
            cluster
                .Setup(m => m.Nodes)
                .Returns(new IClusterNode[] { node1.Object, node2.Object, node3.Object });

            var status = new Status
            {
                VotesReceived = new int[] { 1, 2, 3 }
            };
            var result = VoteResponseChecks.ValidateVotesQuorum(status, cluster.Object);

            result.IsRight.Should().BeTrue();
        }
    }
}


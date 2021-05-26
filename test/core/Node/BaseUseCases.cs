﻿using Moq;
using NUnit.Framework;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftCoreTest.Node
{
    public class BaseUseCases
    {
        protected Agent _sut;
        protected Mock<ICluster> _cluster;
        protected Mock<IElection> _election;

        [SetUp]
        public void SetUp()
        {
            _cluster = new Mock<ICluster>();
            _election = new Mock<IElection>();
            _sut = Agent.Create(_cluster.Object,
                                _election.Object);
        }

        protected Descriptor UseCase_NodeAsLeader()
        {
            var node1 = new Mock<INode>();
            node1.Setup(m => m.Id).Returns(1);
            var node2 = new Mock<INode>();
            node2.Setup(m => m.Id).Returns(2);
            var node3 = new Mock<INode>();
            node3.Setup(m => m.Id).Returns(99);

            _cluster
                .Setup(m => m.Nodes)
                .Returns(new INode[] { node1.Object, node2.Object, node3.Object });

            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Descriptor
            {
                CurrentTerm = 10,
                VotedFor = 1,
                Log = new LogEntry[] { new LogEntry { Term = 9 }, new LogEntry { Term = 10 } },
                CommitLenght = 0,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnRecoverFromCrash(nodeConfig, descriptor);
            descriptor = _sut.OnLeaderHasFailed();
            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 1,
                CurrentTerm = 11,
                Granted = true
            };
            descriptor = _sut.OnReceivedVoteResponse(message);
            message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 2,
                CurrentTerm = 11,
                Granted = true
            };
            return _sut.OnReceivedVoteResponse(message);
        }
    }
}

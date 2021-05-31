using Moq;
using NUnit.Framework;
using RaftCore.Adapters;
using RaftCore.Cluster;
using RaftCore.Models;
using RaftCore.Node;
using System.Collections.Generic;

namespace RaftTest.Core
{
    public class BaseUseCases
    {
        protected Agent _sut;
        protected Mock<ICluster> _cluster;
        protected Mock<IElection> _election;
        protected Mock<IApplication> _application;
        protected Mock<INode> _node1;
        protected Mock<INode> _node2;
        protected Mock<INode> _node3;

        [SetUp]
        public void SetUp()
        {
            _node1 = new Mock<INode>();
            _node1.Setup(m => m.Id).Returns(1);

            _node2 = new Mock<INode>();
            _node2.Setup(m => m.Id).Returns(2);
            
            _node3 = new Mock<INode>();
            _node3.Setup(m => m.Id).Returns(3);

            _cluster = new Mock<ICluster>();
            _election = new Mock<IElection>();
            _application = new Mock<IApplication>();
            _sut = Agent.Create(_cluster.Object,
                                _election.Object,
                                _application.Object);
            ResetCluster();
        }

        protected void ResetCluster()
        {
            _cluster.Reset();
            _cluster
                .Setup(m => m.Nodes)
                .Returns(new INode[] { _node1.Object, _node2.Object, _node3.Object });
        }

        protected Status UseNodeAsLeader()
        {

            var nodeConfig = new NodeConfiguration
            {
                Id = 42
            };

            var descriptor = new Status
            {
                CurrentTerm = 10,
                VotedFor = 1,
                Log = new LogEntry[] {
                        new LogEntry { Term = 6 },
                        new LogEntry { Term = 7 },
                        new LogEntry { Term = 8 },
                        new LogEntry { Term = 9 }, 
                        new LogEntry { Term = 10 },
                        new LogEntry { Term = 11 } },
                CommitLenght = 6,
                CurrentRole = States.Leader,
                CurrentLeader = 2,
                VotesReceived = new int[] { },
                SentLength = new Dictionary<int, int>(),
                AckedLength = new Dictionary<int, int>()
            };

            _ = _sut.OnInitialise(nodeConfig, descriptor);
            _ = _sut.OnLeaderHasFailed();

            var message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 1,
                CurrentTerm = 11,
                Granted = true
            };
            _ = _sut.OnReceivedVoteResponse(message);

            message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 2,
                CurrentTerm = 11,
                Granted = true
            };
            _ = _sut.OnReceivedVoteResponse(message);

            message = new VoteResponseMessage
            {
                Type = MessageType.VoteResponse,
                NodeId = 3,
                CurrentTerm = 11,
                Granted = true
            };
            return _sut.OnReceivedVoteResponse(message);
        }

        protected Status UseNodeAsFollower()
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

            var descriptor = new Status
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

            return _sut.OnInitialise(nodeConfig, descriptor);
        }
    }
}

﻿using FluentAssertions;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Models;

namespace RaftCoreTest.raft.Node
{
    [TestFixture]
    public class MessageSerializerTests
    {
        private MessageSerializer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new MessageSerializer();
        }

        [TestCase(10)]
        [TestCase(-1)]
        public void Deserialize_WhenWrongType_NoneMessage(int type)
            => _sut
                .Deserialize(type, string.Empty)
                .Should()
                .BeEquivalentTo(new Message { Type = MessageType.None } );

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Deserialize_WhenWrongText_NoneMessage(int type)
            => _sut
                .Deserialize(type, "wrong-json")
                .Should()
                .BeEquivalentTo(new Message { Type = MessageType.None } );

        [Test]
        public void Deserialize_LogRequest_LogRequestMessage()
        {
            var json = "{\"Type\":3, \"LeaderId\": 1,\"Term\": 2,\"LogLength\": 3,\"LogTerm\": 4,\"CommitLength\": 5,\"Entries\":[{\"Message\":{\"Type\":3},\"Term\":6}]}";

            var result = _sut.Deserialize((int)MessageType.LogRequest, json) as LogRequestMessage;

            result.Should().NotBeNull();
            result.Type.Should().Be(MessageType.LogRequest);
            result.LeaderId.Should().Be(1);
            result.Term.Should().Be(2);
            result.LogLength.Should().Be(3);
            result.LogTerm.Should().Be(4);
            result.CommitLength.Should().Be(5);
            result.Entries.Should().HaveCount(1);
        }

        [Test]
        public void Deserialize_LogResponse_LogResponseMessage()
        {
            var json = "{\"Type\":4, \"NodeId\": 1,\"Term\": 2,\"Ack\": 3,\"Success\": true}";

            var result = _sut.Deserialize((int)MessageType.LogResponse, json) as LogResponseMessage;

            result.Should().NotBeNull();
            result.Type.Should().Be(MessageType.LogResponse);
            result.NodeId.Should().Be(1);
            result.Term.Should().Be(2);
            result.Ack.Should().Be(3);
            result.Success.Should().Be(true);
        }

        [Test]
        public void Deserialize_VoteRequest_VoteRequestMessage()
        {
            var json = "{\"Type\":1, \"NodeId\": 1,\"CurrentTerm\": 2,\"LogLength\": 3,\"LastTerm\": 4}";

            var result = _sut.Deserialize((int)MessageType.VoteRequest, json) as VoteRequestMessage;

            result.Should().NotBeNull();
            result.Type.Should().Be(MessageType.VoteRequest);
            result.NodeId.Should().Be(1);
            result.CurrentTerm.Should().Be(2);
            result.LogLength.Should().Be(3);
            result.LastTerm.Should().Be(4);
        }

        [Test]
        public void Deserialize_VoteResponse_VoteResponseMessage()
        {
            var json = "{\"Type\":2, \"NodeId\": 1,\"CurrentTerm\": 2,\"Granted\": true}";

            var result = _sut.Deserialize((int)MessageType.VoteResponse, json) as VoteResponseMessage;

            result.Should().NotBeNull();
            result.Type.Should().Be(MessageType.VoteResponse);
            result.NodeId.Should().Be(1);
            result.CurrentTerm.Should().Be(2);
            result.Granted.Should().Be(true);
        }

        [Test]
        public void Deserialize_Application_ApplicationMessage()
        {
            var result = _sut.Deserialize((int)MessageType.Application, string.Empty);

            result.Should().BeOfType(typeof(Message));
            result.Type.Should().Be(MessageType.Application);
        }

        [Test]
        public void Deserialize_None_NoneMessage()
        {
            var result = _sut.Deserialize((int)MessageType.None, string.Empty);

            result.Should().BeOfType(typeof(Message));
            result.Type.Should().Be(MessageType.None);
        }
    }
}
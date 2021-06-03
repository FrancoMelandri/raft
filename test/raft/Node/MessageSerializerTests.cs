using FluentAssertions;
using NUnit.Framework;
using Raft.Node;
using RaftCore.Models;

namespace RaftCoreTest.raft.Node
{
    [TestFixture]
    public class MessageSerializerTests
    {
        [Test]
        public void Deserialize_WhenWrongType_NoneMessage()
            => new MessageSerializer()
                .Deserialize(0, string.Empty)
                .Should()
                .BeEquivalentTo(new Message { Type = MessageType.None } );
    }
}

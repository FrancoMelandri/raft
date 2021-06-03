using RaftCore.Adapters;
using RaftCore.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using TinyFp;
using static TinyFp.Prelude;

namespace Raft.Node
{
    public class MessageSerializer : IMessageSerializer
    {
        private Dictionary<MessageType, Func<string, Message>> _deserializers;

        public MessageSerializer()
        {
            _deserializers = new Dictionary<MessageType, Func<string, Message>>
            {
                { MessageType.None, data => new Message { Type = MessageType.None} },
                { MessageType.LogRequest, data =>  JsonSerializer.Deserialize<LogRequestMessage>(data) },
                { MessageType.LogResponse, data =>  JsonSerializer.Deserialize<LogResponseMessage>(data) },
                { MessageType.VoteRequest, data =>  JsonSerializer.Deserialize<VoteRequestMessage>(data) },
                { MessageType.VoteResponse, data =>  JsonSerializer.Deserialize<VoteResponseMessage>(data) },
                { MessageType.Application, data =>  ApplicationMessageDeserialize(data) },
            };
        }

        public Either<string, MessageType> IsValidType(int type)
            => type >= (int)MessageType.None &&
               type <= (int)MessageType.Application ?
            Right<string, MessageType>((MessageType)type) :
            Left<string, MessageType>("");

        public Message Deserialize(int type, string message)
            => IsValidType(type)
                .Match(_ => InternalDeserialize(_, message),
                       _ => new Message { Type = MessageType.None } );

        private Message InternalDeserialize(MessageType type, string message)
            => Try(() => _deserializers[type](message))
                .Match(_ => _, ex => new Message { Type = MessageType.None });

        protected virtual Message ApplicationMessageDeserialize(string message)
            => new() { Type = MessageType.Application };
    }
}

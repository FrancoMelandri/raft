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
        private Dictionary<MessageType, Func<Message, string>> _serializers;

        public MessageSerializer()
        {
            _deserializers = new Dictionary<MessageType, Func<string, Message>>
            {
                { MessageType.None, data => new Message { Type = MessageType.None} },
                { MessageType.VoteRequest, data =>  JsonSerializer.Deserialize<VoteRequestMessage>(data) },
                { MessageType.VoteResponse, data =>  JsonSerializer.Deserialize<VoteResponseMessage>(data) },
                { MessageType.LogRequest, data =>  JsonSerializer.Deserialize<LogRequestMessage>(data) },
                { MessageType.LogResponse, data =>  JsonSerializer.Deserialize<LogResponseMessage>(data) },
                { MessageType.Application, data =>  ApplicationMessageDeserialize(data) },
            };

            _serializers = new Dictionary<MessageType, Func<Message, string>>
            {
                { MessageType.None, data => JsonSerializer.Serialize(data) },
                { MessageType.VoteRequest, data =>  JsonSerializer.Serialize(data as VoteRequestMessage) },
                { MessageType.VoteResponse, data =>  JsonSerializer.Serialize(data as VoteResponseMessage) },
                { MessageType.LogRequest, data =>  JsonSerializer.Serialize(data as LogRequestMessage) },
                { MessageType.LogResponse, data =>  JsonSerializer.Serialize(data as LogResponseMessage) },
                { MessageType.Application, data =>  ApplicationMessageSerialize(data) },
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

        public string Serialize(Message message)
            => IsValidType((int)message.Type)
                .Match(_ => InternalSerialize(_, message),
                       _ => string.Empty);

        private Message InternalDeserialize(MessageType type, string message)
            => Try(() => _deserializers[type](message))
                .Match(_ => _, ex => new Message { Type = MessageType.None });

        protected virtual Message ApplicationMessageDeserialize(string message)
            => new() { Type = MessageType.Application };

        protected virtual string ApplicationMessageSerialize(Message message)
            => JsonSerializer.Serialize(message);

        private string InternalSerialize(MessageType type, Message message)
            => Try(() => _serializers[type](message))
                .Match(_ => _, ex => string.Empty);

    }
}

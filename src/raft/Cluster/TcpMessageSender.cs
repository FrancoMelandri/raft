using RaftCore.Adapters;
using RaftCore.Models;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using TinyFp;
using TinyFp.Extensions;
using static System.Convert;
using static Raft.Constants.Nodes;
using static Raft.Constants.Messages;
using static Raft.Constants.Logs;
using static TinyFp.Prelude;
using static System.Text.Encoding;

namespace Raft.Cluster
{
    [ExcludeFromCodeCoverage]
    public class TcpMessageSender : IMessageSender
    {
        private readonly ClusterConfiguration _clusterConfiguration;
        private readonly IMessageSerializer _messageSerializer;
        private readonly ILogger _logger;
        private TcpClient _tcpClient;

        public TcpMessageSender(ClusterConfiguration clusterConfiguration,
                                IMessageSerializer messageSerializer,
                                ILogger logger)
        {
            _tcpClient = new TcpClient();
            _clusterConfiguration = clusterConfiguration;
            _messageSerializer = messageSerializer;
            _logger = logger;
        }

        public Unit SendMessage(Message message)
            => Try(() => InternalSendMessage(message))
                .OnFail(ex => Unit.Default
                                .Tee(_ => _logger.Error($"{ERROR_SENDING_MESSAGE} {_messageSerializer.Serialize(message)}")));

        public Unit Start(int nodeId)
            => _clusterConfiguration
                .Nodes
                .FirstOrDefault(_ => _.Id == nodeId)
                .ToOption()
                .Map(_ => (Address: _.Properties[ADDRESS], Port: ToInt32(_.Properties[PORT])))
                .Match(_ => Connect(_.Address, _.Port),
                      () => Unit.Default);

        public Unit Stop()
            => Unit.Default;

        private Unit Connect(string address, int port)
            => Try(() => Unit.Default
                        .Tee(__ => _tcpClient.Connect(address, port)))
                .OnFail(ex => Unit.Default
                                .Tee(_ => _logger.Error($"{ERROR_CONNECTING_TO_NODE} {address}:{port}")));

        private Unit InternalSendMessage(Message message)
            => IsTcpClientConnected()
                .Match(_ => _messageSerializer.Serialize(message)
                                .Map(_ => (Msg: _, Buffer: _.Length.ToString().PadLeft(16, PADDING_CHAR)))
                                .Map(_ => (Msg: _.Msg, Buffer: UTF8.GetBytes(_.Buffer)))
                                .Tee(_ => _tcpClient.GetStream().Write(_.Buffer, 0, _.Buffer.Length))
                                .Map(_ => (Msg: _.Msg, Buffer: UTF8.GetBytes(((int)message.Type).ToString())))
                                .Tee(_ => _tcpClient.GetStream().Write(_.Buffer, 0, _.Buffer.Length))
                                .Map(_ => (Msg: _.Msg, Buffer: UTF8.GetBytes(_.Msg)))
                                .Tee(_ => _tcpClient.GetStream().Write(_.Buffer, 0, _.Buffer.Length))
                                .Map(_ => Unit.Default),
                    _ => Unit.Default);

        private Either<string, TcpClient> IsTcpClientConnected()
            => _tcpClient.Connected ?
                Right<string, TcpClient>(_tcpClient) :
                Left<string, TcpClient>("");
    }
}

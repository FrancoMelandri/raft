using RaftCore.Adapters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TinyFp;
using TinyFp.Extensions;
using RaftCore.Models;
using RaftCore.Cluster;
using static System.Convert;
using static System.Threading.Tasks.Task;
using static Raft.Constants.Messages;
using static System.Text.Encoding;
using static TinyFp.Prelude;

namespace Raft.Node
{
    [ExcludeFromCodeCoverage]
    public class TcpMessageListener : IMessageListener
    {
        private const string LOCAL_PORT = "Port";
        private const string LOCAL_HOST = "127.0.0.1";

        private readonly LocalNodeConfiguration _localNodeConfiguration;
        private readonly IMessageSerializer _messageDeserializer;
        private IMessageObserver _messageObserver;
        private TcpListener _tcpListener;
        private Task _listener;

        public TcpMessageListener(LocalNodeConfiguration localNodeConfiguration,
                                  IMessageSerializer messageDeserializer)
        {
            _localNodeConfiguration = localNodeConfiguration;
            _messageDeserializer = messageDeserializer;
        }

        public Unit Start(IMessageObserver messageObserver)
            => Unit.Default
                .Tee(_ => _messageObserver = messageObserver)
                .Tee(_ => StartListener());

        public Unit Stop()
            => Unit.Default
                .Tee(_ => StopListening());

        private Unit StartListener()
            => Unit.Default
                .Tee (_ =>
                    (Address: IPAddress.Parse(LOCAL_HOST), Port: ToInt32(_localNodeConfiguration.Properties[LOCAL_PORT]))
                    .Map(_ => _tcpListener = new TcpListener(_.Address, _.Port))
                    .Tee(_ => _tcpListener.Start())
                    .Tee(_ => _listener = Factory.StartNew(() => Listen())));

        private Unit StopListening()
            => Unit.Default.Tee (_ => _listener.Wait(0));

        private void Listen()
            => _tcpListener
                .AcceptTcpClient()
                .Tee(_ => _listener = Factory.StartNew(() => Listen()))
                .Map(_ => HandleIncomingMessage(_));

        private Unit HandleIncomingMessage(TcpClient client)
            => GetMessageSize(client)
                .Map(_ => (Size: _, Type: GetMessageType(client).OnNone(0)))
                .Bind(_ => GetMessage(_.Size, _.Type, client))
                .Match(_ => _, () => new Message { Type = MessageType.None })
                .Tee(_ => client.Close())
                .Map(_ => _messageObserver.NotifyMessage(_));

        private static Option<int> GetMessageSize(TcpClient client)
            => Try(() => new byte[HEADER_SIZE]
                            .Tee(_ => client.GetStream().Read(_, 0, HEADER_SIZE))
                            .Map(_ => UTF8.GetString(_))
                            .Map(_ => _.Replace(PADDING_CHAR, REPLACING_CHAR))
                            .Map(_ => ToInt32(_)))
               .Match(_ => Option<int>.Some(_),
                      _ => Option<int>.None());

        private static Option<int> GetMessageType(TcpClient client)
            => Try(() => new byte[TYPE_SIZE]
                            .Tee(_ => client.GetStream().Read(_, 0, TYPE_SIZE))
                            .Map(_ => UTF8.GetString(_))
                            .Map(_ => ToInt32(_)))
               .Match(_ => Option<int>.Some(_),
                      _ => Option<int>.None());

        private Option<Message> GetMessage(int size, int type, TcpClient client)
            => Try(() => new byte[size]
                            .Tee(_ => client.GetStream().Read(_, 0, size))
                            .Map(_ => UTF8.GetString(_))
                            .Map(_ => _messageDeserializer.Deserialize(type, _)))
               .Match(_ => Option<Message>.Some(_),
                      _ => Option<Message>.None());
    }
}

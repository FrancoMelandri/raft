using RaftCore.Adapters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TinyFp;
using TinyFp.Extensions;
using static System.Convert;
using static System.Threading.Tasks.Task;
using static Raft.Constants.Messages;
using static System.Text.Encoding;

namespace Raft.Node
{
    [ExcludeFromCodeCoverage]
    public class TcpMessageListener : IMessageListener
    {
        private const string LOCAL_PORT = "Port";
        private const string LOCAL_HOST = "127.0.0.1";

        private readonly LocalNodeConfiguration _localNodeConfiguration;
        private TcpListener _tcpListener;

        private Task _listener;

        public TcpMessageListener(LocalNodeConfiguration localNodeConfiguration)
        {
            _localNodeConfiguration = localNodeConfiguration;
        }

        public Unit Start()
            => Unit.Default
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
                .Map(_ => HandleIncomingMessage(_))
                .Tee(_ => _listener = Task.Factory.StartNew(() => Listen()));

        private Unit HandleIncomingMessage(TcpClient client)
            => GetHeader(client)
                .Tee(_ => client.Close())
                .Map(_ => Unit.Default);

        private static int GetHeader(TcpClient client)
            => new byte[HEADER_SIZE]
                .Tee(_ => client.GetStream().Read(_, 0, HEADER_SIZE))
                .Map(_ => UTF8.GetString(_))
                .Map(_ => _.Replace(PADDING_CHAR, REPLACING_CHAR))
                .Map(_ => ToInt32(_));
    }
}

using System.Net;
using System.Net.Sockets;
using static System.Threading.Tasks.Task;
using static System.Console;
using static System.Text.Encoding;
using static System.Convert;

namespace mockfollower
{
    class Program
    {
        static TcpListener _tcpListener;

        static void Main(string[] args)
        {
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3001);
            _tcpListener.Start();
            _ = Factory.StartNew(() => Listen());

            ReadKey();
        }

        private static void Listen()
        {
            WriteLine("Listening....");
            var client = _tcpListener.AcceptTcpClient();
            WriteLine($"Accepting client {client.Client.RemoteEndPoint}");


            _ = Factory.StartNew(() => Listen());

            var buffer = new byte[16];
            client.GetStream().Read(buffer, 0, 16);
            
            var size = ToInt32(UTF8.GetString(buffer).Replace(" ", ""));

            buffer = new byte[1];
            client.GetStream().Read(buffer, 0, 1);
            var type = ToInt32( UTF8.GetString(buffer));


            buffer = new byte[size];
            client.GetStream().Read(buffer, 0, size);
            var body = UTF8.GetString(buffer);

            WriteLine($"Incoming message: {type}");
            WriteLine($"With body: {body}");

            if (type == 1)
                SendResponseToNode();
        }

        private static void SendResponseToNode()
        {
            TcpClient client = new();
            client.Connect("localhost", 3000);

            var type = 2;
            var message = "{\"Type\":2,\"NodeId\":2,\"CurrentTerm\":1,\"Granted\":true}";

            var header = message.Length.ToString().PadLeft(16, ' ');
            var buffer = UTF8.GetBytes(header);
            client.GetStream().Write(buffer, 0, buffer.Length);

            buffer = UTF8.GetBytes(type.ToString());
            client.GetStream().Write(buffer, 0, 1);

            buffer = UTF8.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}

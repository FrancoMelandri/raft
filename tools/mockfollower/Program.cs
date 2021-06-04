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

            var header = new byte[16];
            client.GetStream().Read(header, 0, 16);
            
            var size = ToInt32(UTF8.GetString(header).Replace(" ", ""));

            var type = new byte[1];
            client.GetStream().Read(type, 0, 1);

            var body = new byte[size];
            client.GetStream().Read(body, 0, size);

            WriteLine($"Incoming message: {UTF8.GetString(type)}");
            WriteLine($"With body: {UTF8.GetString(body)}");
        }
    }
}

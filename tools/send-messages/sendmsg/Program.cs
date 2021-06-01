using System.Net.Sockets;

namespace sendmsg
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new();
            client.Connect("localhost", 3000);

            var header = "              10";
            var buffer = System.Text.Encoding.UTF8.GetBytes(header);
            client.GetStream().Write(buffer, 0, 16);
        }
    }
}

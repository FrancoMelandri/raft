using System.Net.Sockets;

namespace sendmsg
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(5000);
            SendMessage("{\"Type\":1}");

            System.Threading.Thread.Sleep(5000);
            SendMessage("{\"Type\":2}");

            System.Threading.Thread.Sleep(5000);
            SendMessage("{\"Type\":3}");
        }

        private static void SendMessage(string message)
        {
            TcpClient client = new();
            client.Connect("localhost", 3000);

            var header = message.Length.ToString().PadLeft(16, ' ');
            var buffer = System.Text.Encoding.UTF8.GetBytes(header);
            client.GetStream().Write(buffer, 0, buffer.Length);

            buffer = System.Text.Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}

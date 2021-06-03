using System.Net.Sockets;

namespace sendmsg
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(5000);
            SendMessage(1, "{\"Type\":1}");

            System.Threading.Thread.Sleep(5000);
            SendMessage(2, "{\"Type\":2}");

            System.Threading.Thread.Sleep(5000);
            SendMessage(3, "{\"Type\":3}");
        }

        private static void SendMessage(int type, string message)
        {
            TcpClient client = new();
            client.Connect("localhost", 3000);

            var header = message.Length.ToString().PadLeft(16, ' ');
            var buffer = System.Text.Encoding.UTF8.GetBytes(header);
            client.GetStream().Write(buffer, 0, buffer.Length);

            buffer = System.Text.Encoding.UTF8.GetBytes(type.ToString());
            client.GetStream().Write(buffer, 0, 1);

            buffer = System.Text.Encoding.UTF8.GetBytes(message);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}

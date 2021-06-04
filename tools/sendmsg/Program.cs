using System.Net.Sockets;
using static System.Text.Encoding;
using static System.Threading.Thread;

namespace sendmsg
{
    class Program
    {
        static void Main(string[] args)
        {
            Sleep(5000);
            SendMessage(1, "{\"Type\":1}");

            Sleep(5000);
            SendMessage(2, "{\"Type\":2}");

            Sleep(5000);
            SendMessage(3, "{\"Type\":3}");

            Sleep(5000);
            SendMessage(5, "{\"Type\":5, \"CurrentStatus\": \"INIT\"}");
        }

        private static void SendMessage(int type, string message)
        {
            TcpClient client = new();
            client.Connect("localhost", 3000);

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

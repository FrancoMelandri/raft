using System;
using System.Net.Sockets;

namespace mockleader
{
    class Program
    {
        static void Main(string[] args)
        {
            var count = 0;
            while (true)
            {
                SendMessage(count++, 3, "{\"Type\":3}");
                System.Threading.Tasks.Task.Delay(100);
            }
        }

        private static void SendMessage(int count, int type, string message)
        {
            try
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

                Console.WriteLine($"{count} - {DateTime.Now.ToString("HH:mm:ss")}: Log request sent");
            }
            catch (Exception ex)            
            {
                Console.WriteLine($"{count} - {DateTime.Now.ToString("HH:mm:ss")} : Exception {ex.Message}");
            }
        }
    }
}

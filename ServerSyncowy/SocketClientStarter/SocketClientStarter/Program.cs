using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketClientStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = null;
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddr = null;
            IPAddress.TryParse("89.229.95.152", out ipaddr);
            int intPort = 16010;
            string inputString;
            client.Connect(ipaddr, intPort);

            while (true)
            {
                inputString = Console.ReadLine();

                inputString = inputString.Trim();

                byte[] buffSend = Encoding.ASCII.GetBytes(inputString);

                client.Send(buffSend);

                byte[] buffReceived = new byte[1000000];
                int numberOfReceivedBytes = client.Receive(buffReceived);

                string receivedText = Encoding.ASCII.GetString(buffReceived, 0, numberOfReceivedBytes);

                Console.WriteLine(receivedText);

                numberOfReceivedBytes = 0;
                Array.Clear(buffReceived, 0, buffReceived.Length);
            }

            if(client !=null)
            {
                if(client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client.Dispose();
                }
            }
            Console.ReadKey();
        }
    }
}

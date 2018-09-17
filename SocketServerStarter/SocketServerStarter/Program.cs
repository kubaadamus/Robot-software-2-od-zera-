using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace SocketServerStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipaddr = IPAddress.Any;

            IPEndPoint ipep = new IPEndPoint(ipaddr, 16010);

            listenerSocket.Bind(ipep);

            listenerSocket.Listen(5);

            Console.WriteLine("About to accept incoming connection");

            Socket client = listenerSocket.Accept();

            Console.WriteLine("Client connected. " + client.ToString() + " IP EndPoint: " + client.RemoteEndPoint.ToString());

            byte[] buff = new byte[786432];

            int numberOfReceivedBytes = 0;


            while(true)
            {
                numberOfReceivedBytes = client.Receive(buff);

                Console.WriteLine("number of received bytes" + numberOfReceivedBytes);

                Console.WriteLine("Data sent by client is: " + buff);

                string receivedText = Encoding.ASCII.GetString(buff, 0, numberOfReceivedBytes);

                Console.WriteLine("Data sent by client: " + receivedText);

                client.Send(buff);

                if (receivedText == "quit")
                {
                    break;
                }

                Array.Clear(buff, 0, buff.Length);
                numberOfReceivedBytes = 0;


            }
            Console.ReadKey();
        }
    }
}

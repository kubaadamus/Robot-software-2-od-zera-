﻿using System;
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
            int numberOfReceivedBytes = 0;
            byte[] buffIn = new byte[786432];

            Console.WriteLine("About to accept incoming connection");
            Socket client = listenerSocket.Accept();
            Console.WriteLine("Client connected. " + client.ToString() + " IP EndPoint: " + client.RemoteEndPoint.ToString());

                while (true)
                {
                    numberOfReceivedBytes = client.Receive(buffIn); // tutaj czekaj na dane od klienta, ten element blokuje kod

                    string receivedText = Encoding.ASCII.GetString(buffIn, 0, numberOfReceivedBytes); // zamien odebrane dane na string

                    Console.WriteLine("Data sent by client: " + receivedText);

                    byte[] buffOut = Encoding.ASCII.GetBytes("serwer wysyla");

                    client.Send(buffOut); //wyslij dane do klienta

                    Array.Clear(buffIn, 0, buffIn.Length);  // wyczyść buff i przygotuj na nowe dane
                    numberOfReceivedBytes = 0;          //wyzeruj licznik odebranych bajtów

                }
           
            Console.ReadKey();
        }
    }
}

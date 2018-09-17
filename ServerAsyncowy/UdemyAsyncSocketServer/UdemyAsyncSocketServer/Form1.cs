using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace UdemyAsyncSocketServer
{
    public partial class Form1 : Form
    {
        IPAddress mIP;
        int mPort;
        TcpListener mTCPListener;
        public Form1()
        {
            InitializeComponent();
            StartListeningForIncomingConnection();
        }
        public async void StartListeningForIncomingConnection(IPAddress ipaddr = null, int port = 16010)
        {
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }

            if (port <= 0)
            {
                port = 23000;
            }

            mIP = ipaddr;
            mPort = port;

            System.Diagnostics.Debug.WriteLine(string.Format("IP Address: {0} - Port: {1}", mIP.ToString(), mPort));

            mTCPListener = new TcpListener(mIP, mPort);
            mTCPListener.Start();

            var returnedByAccept = await mTCPListener.AcceptTcpClientAsync();

            System.Diagnostics.Debug.WriteLine("Client connected successfully: " + returnedByAccept.ToString());

        }
    }
}

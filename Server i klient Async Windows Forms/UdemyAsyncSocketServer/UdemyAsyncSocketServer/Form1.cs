using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LahoreSocketAsync;
using System.IO;
using System.Diagnostics;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UdemyAsyncSocketServer
{
    public partial class Form1 : Form
    {
        LahoreSocketServer mServer;
        //zmienne kamery
        private FilterInfoCollection videoDevicesList;
        private IVideoSource videoSource;
        Bitmap bitmap;
        VideoCaptureDevice vidcapdev = new VideoCaptureDevice();
        int ClientsAmount = 0;

        public Form1()
        {
            InitializeComponent();
            mServer = new LahoreSocketServer();
            mServer.RaiseClientConnectedEvent += HandleClientConnected;
            mServer.RaiseTextReceivedEvent += HandleTextReceived;
            mServer.RaiseClientDisconnectedEvent += HandleClientDisconnected;

            //INICJALIZACJA KAMERY
            // get list of video devices
            videoDevicesList = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevicesList)
            {
                cmbVideoSource.Items.Add(videoDevice.Name);
            }
            if (cmbVideoSource.Items.Count > 0)
            {
                cmbVideoSource.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("No video sources found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // stop the camera on window close
            this.Closing += Form1_Closing;
        }

        private void btnAcceptIncomingAsync_Click(object sender, EventArgs e)
        {
            mServer.StartListeningForIncomingConnection();
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            mServer.SendToAll(txtMessage.Text.Trim());
            
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            mServer.StopServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mServer.StopServer();
        }

        void HandleClientConnected(object sender, ClientConnectedEventArgs ccea)
        {
            txtConsole.AppendText(string.Format("{0} - New client connected: {1}{2}", 
                DateTime.Now, ccea.NewClient, Environment.NewLine));
            ClientsAmount += 1;
            clientsAmount.Text = ClientsAmount.ToString();
        }

        void HandleTextReceived(object sender, TextReceivedEventArgs trea)
        {
            txtConsole.AppendText(
                string.Format(
                    "{0} - Received from {2}: {1}{3}",
                    DateTime.Now,
                    trea.TextReceived, 
                    trea.ClientWhoSentText,
                    Environment.NewLine));
        }

        void HandleClientDisconnected(object sender, ConnectionDisconnectedEventArgs cdea)
        {
            if (!txtConsole.IsDisposed)
            {
                txtConsole.AppendText(string.Format("{0} - Client Disconnected: {1}\r\n",
                    DateTime.Now, cdea.DisconnectedPeer));
            }
            ClientsAmount -= 1;
            clientsAmount.Text = ClientsAmount.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //txtConsole.AppendText(DateTime.Now.ToString() + "wyslano klatke" +"\n");
            //mServer.SendToAll(DateTime.Now.ToString());

            //WYSYŁANIE KLATKI DO KLIENTA
            byte[] imgToSend = ImageToByte(bitmap);
            mServer.SendToAll(imgToSend);
            
            txtConsole.AppendText(DateTime.Now.ToString() + " " + imgToSend.Length + "\n");
            pictureBox2.Image = CopyDataToBitmap(ImageToByte(bitmap));
        }

        private void startTimerButton_Click(object sender, EventArgs e)
        {


            if(!timer1.Enabled)
            {
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
            }
        }


        //FUNKCJE KAMERY
        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            // signal to stop
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }

        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = (Bitmap)eventArgs.Frame.Clone();

            pictureBox1.Image = bitmap;

        }




        private void btnStart_Click_1(object sender, EventArgs e)
        {
            videoSource = new VideoCaptureDevice(videoDevicesList[cmbVideoSource.SelectedIndex].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.Start();


        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            videoSource.SignalToStop();
            if (videoSource != null && videoSource.IsRunning && pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
        }

        public static byte[] ImageToByte(Bitmap img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public Bitmap CopyDataToBitmap(byte[] data)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

            return (Bitmap)tc.ConvertFrom(data);
        }
    }
}

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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Ports;

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
        SerialPort serial1;
        string inString = "";

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
            this.Closing += Form1_Closing;

            InicjalizujSerial();
            serial1.DataReceived += new SerialDataReceivedEventHandler(port_OnReceiveDatazz);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            byte[] imgToSend = ImageToByte(bitmap);
            mServer.SendToAll(imgToSend);
            txtConsole.AppendText(inString);

        }
        //======================================================================================== F U N K C J E  K A M E R Y ===================================================================
        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            bitmap = (Bitmap)eventArgs.Frame.Clone();

            pictureBox1.Image = bitmap;
        
        }
        //========================================================================= I M A G E  T O  B Y T E  A R R A Y  I  S P O W R O T E M ====================================================
        public static byte[] ImageToByte(Bitmap img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        public Bitmap CopyDataToBitmap(byte[] data)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));

            return (Bitmap)tc.ConvertFrom(data);
        }
        long sprawdzRozmiarObiektu(object a)
        {
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, a);
                return s.Length;
            }
        }
        //=========================================================================================== K O M P R E S J A =========================================================================
        private Image GetCompressedBitmap(Bitmap bmp, long quality)
        {
            using (var mss = new MemoryStream())
            {
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = qualityParam;
                bmp.Save(mss, imageCodec, parameters);
                return Image.FromStream(mss);
            }
        }
        //======================================================================================= R E S Z T A   R Z E C Z Y =====================================================================
        private void btnAcceptIncomingAsync_Click(object sender, EventArgs e)
        {
            mServer.StartListeningForIncomingConnection();
        }
        private void btnSendAll_Click(object sender, EventArgs e)
        {
            mServer.SendToAll(txtMessage.Text.Trim());
            WyslijDoArduino(txtMessage.Text.Trim());

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
            txtConsole.AppendText(trea.TextReceived + Environment.NewLine);
            txtConsole.AppendText(System.Environment.NewLine);
            WyslijDoArduino(trea.TextReceived);
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
        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            // signal to stop
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
        }
        private void startTimerButton_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
            }
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

        void InicjalizujSerial()
        {

            serial1 = new SerialPort();
            serial1.PortName = "COM4";
            serial1.Parity = Parity.None;
            serial1.BaudRate = 115200;
            serial1.DataBits = 8;
            serial1.StopBits = StopBits.One;
            if (!serial1.IsOpen && serial1 != null)
            {
                serial1.Open();
                serial1.ReadTimeout = 2000;
                serial1.WriteTimeout = 1000;
            }
            serial1.BaseStream.Flush();
            serial1.DiscardInBuffer();
            serial1.DiscardOutBuffer();
        }

        void WyslijDoArduino(string inputString)
        {
            serial1.Write(inputString);
            string dane;
            
        }

        private static void port_OnReceiveDatazz(object sender,
                                          SerialDataReceivedEventArgs e)
        {
            SerialPort spL = (SerialPort)sender;
            byte[] buf = new byte[spL.BytesToRead];
            Console.WriteLine("DATA RECEIVED!");
            spL.Read(buf, 0, buf.Length);
            string myString = System.Text.Encoding.ASCII.GetString(buf).Trim();
            Console.Write(myString);
            Console.WriteLine();
        }
    }
}


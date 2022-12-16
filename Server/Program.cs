using Common.CommonData;
using Common.Models;
using Common.Requests;
using Common.Responses;
using Server.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    public class Program
    {
        public static UdpClient UdpClient = new UdpClient();
        public static TcpClient Client = null;

        public static NetworkStream Stream = null;

        public static BinaryReader BinaryReader = null;

        public static BinaryWriter BinaryWriter = null;

        public static IPAddress ServerIpAddress = null;

        public static DeviceInfo ServerInfo = new DeviceInfo();

        public static DeviceInfo ClientInfo = null;

        public static TcpListener Listener = null;

        public static void InitializeInistances()
        {

            // Initialize Server Ip Address
            ServerIpAddress = IPAddress.Loopback;

            // Initialize TCP Listener
            //Listener = new TcpListener(ServerIpAddress, GeneralUDPValues.SERVER_PORT_NUMBER);
            Listener = new TcpListener(27001);

            Console.WriteLine("Server launched");
            ConsoleHelper.ShowPropery("Server Ip Address", ServerIpAddress.ToString());
        }

        public static void ConnectionClient()
        {
            // Server's read loop
            string request = string.Empty;
            while (true)
            {
                // Send response for Client send their device info
                BinaryWriter.Write(ConnectionResponses.SEND_DEVICE_INFO);

                // Read request
                request = BinaryReader.ReadString();

                try
                {
                    ClientInfo = JsonSerializer.Deserialize<DeviceInfo>(request);

                    // Check Client device info
                    if (ClientInfo == null)
                    {
                        BinaryWriter.Write(ConnectionResponses.CONNECTION_NOT_ESTABLISHED);
                        break;
                    }

                }
                catch (Exception)
                {
                    BinaryWriter.Write(ConnectionResponses.CONNECTION_NOT_ESTABLISHED);
                    break;
                }

                // Send response to Client connection successfully established
                BinaryWriter.Write(ConnectionResponses.CONNECTION_SUCCESSFULLY_ESTABLISHED);

                // Show connected Client
                ConsoleHelper.ShowClientInfo(ClientInfo);

                break;
            }
        }

        public static void DisconnectionClient()
        {
            if (ClientInfo == null)
                return;

            ConsoleHelper.ShowClientInfo(ClientInfo, ClientInfoType.Disconnected);
            ClientInfo = null;
            Client.Close();
            Stream.Close();
            BinaryWriter.Close();
            BinaryReader.Close();
        }

        public static void SendScreenshot()
        {
            // Initialize Instances
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ClientInfo.IpAddress), GeneralUDPValues.CLIENT_PORT_NUMBER);
            byte[] response = new byte[ushort.MaxValue];


            //Bitmap memoryImage;
            //memoryImage = new Bitmap(1600, 1080);
            //Size s = new Size(memoryImage.Width, memoryImage.Height);
            //Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            //memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);


            //ImageConverter converter = new ImageConverter();
            //var bytes = (byte[])converter.ConvertTo(memoryImage, typeof(byte[]));
            //int a = (bytes.Length / 20000) + 1;
            //int count = 0;
            //while (a >= 0)
            //{
            //    if (a == 0)
            //    {
            //        UdpClient.Send(new byte[2] { 2, 3 }, new byte[2] { 2, 3 }.Length, remoteEP);
            //        break;
            //    }
            //    UdpClient.Send(bytes.Skip(count).Take(20000).ToArray(), bytes.Skip(count).Take(20000).ToArray().Length, remoteEP);
            //    a--;
            //    count += 20000;
            //}



            // Send empty response to client for inilialize endpoint
            response = Encoding.Default.GetBytes(ConnectionResponses.INITIALIZE_ENDPOINT);
            UdpClient.Send(response, response.Length, remoteEP);


            // Get Screenshot
            Bitmap memoryImage;
            memoryImage = new Bitmap(1600, 1080);
            Size s = new Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);


            // Convert Bytes
            ImageConverter converter = new ImageConverter();
            var bytes = (byte[])converter.ConvertTo(memoryImage, typeof(byte[]));


            // Find & Sending number of part
            int numberOfParts = (bytes.Length / 40_000) + 1;
            response = Encoding.Default.GetBytes(numberOfParts.ToString());
            UdpClient.Send(response, response.Length, remoteEP);
            UdpClient.Receive(ref remoteEP);


            // Find & Sending lenght of image byte array
            response = Encoding.Default.GetBytes(bytes.Length.ToString());
            UdpClient.Send(response, response.Length, remoteEP);
            UdpClient.Receive(ref remoteEP);


            // Divide into parts and send
            int sended = 0;
            for (int i = 0; i < numberOfParts; i++)
            {
                response = bytes.Skip(sended).Take(40000).ToArray();
                sended += UdpClient.Send(response, response.Length, remoteEP);
                UdpClient.Receive(ref remoteEP);
            }
        }

        public static void StartServer()
        {
            // Server's main loop
            while (true)
            {
                string request = BinaryReader.ReadString();

                switch (request.ToLower())
                {
                    case ConnectionRequests.CONNECTION_REQUEST:
                        ConnectionClient();
                        break;

                    case ConnectionRequests.DISCONNECTION_REQUEST:
                        DisconnectionClient();
                        return;

                    case ConnectionRequests.GET_SCREENSHOT_REQUEST:
                        SendScreenshot();
                        break;

                    default:
                        break;
                }

            }
        }

        static void Main(string[] args)
        {

            InitializeInistances();

            // Start listening 
            Listener.Start();

            // Server's main loop.
            while (true)
            {
                Client = Listener.AcceptTcpClient();
                Stream = Client.GetStream();
                BinaryReader = new BinaryReader(Stream);
                BinaryWriter = new BinaryWriter(Stream);

                StartServer();
            }

        }

    }
}
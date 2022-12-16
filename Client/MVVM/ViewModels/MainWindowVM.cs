using Client.MVVM.Models;
using Client.MVVM.Views;
using Common.CommonData;
using Common.Models;
using Common.Requests;
using Common.Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using RelayCommand = Client.Commands.RelayCommand;

namespace Client.MVVM.ViewModels
{
    public class MainWindowVM
    {

        #region PropertyChangedEventHandler

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Members

        public System.Windows.Controls.Image ScreenShotArea { get; set; }

        public DeviceInfo DeviceInfo { get; set; } = new DeviceInfo();


        public static TcpClient Client = null;

        public static UdpClient UdpClient = new UdpClient(GeneralUDPValues.CLIENT_PORT_NUMBER);

        public static NetworkStream Stream = null;

        public static BinaryReader BinaryReader = null;

        public static BinaryWriter BinaryWriter = null;


        public InfoBar ConnectionInfoBar { get; set; }
        public Button ConnectButton { get; set; }
        public Button DisposeButton { get; set; }
        public TextBox ServerIpAddress { get; set; }

        #endregion

        #region Commands

        public RelayCommand ConnectCommand { get; set; }

        public RelayCommand DisposeConnectionCommand { get; set; }

        public RelayCommand ExitCommand { get; set; }



        public RelayCommand AboutCommand { get; set; }

        public RelayCommand AboutDeviceCommand { get; set; }

        #endregion

        #region Command Run Methods

        public async void ConnectCommandRun(object param)
        {
            try
            {
                var connectionServer = ConnectServer();
                if (!connectionServer.Successed)
                {
                    await ShowInfobar(connectionServer, InfoBarSeverity.Error, new TimeSpan(0, 0, 3));
                    return;
                }
            }
            catch (Exception)
            {
                await ShowInfobar(GetBaseConnectioErrorInfoBarData(), InfoBarSeverity.Error, new TimeSpan(0, 0, 3));
                return;
            }

            await ShowInfobar("Successfuly connected to the server", "Screen sharing will start automatically in a short time.", InfoBarSeverity.Success, new TimeSpan(0, 0, 3));

            ServerIpAddress.IsEnabled = false;
            ConnectButton.IsEnabled = false;
            DisposeButton.Visibility = Visibility.Visible;
            DeviceInfo.Status = Common.Enums.ClientConnectionStatus.Connected;

            StartScreenShare();
        }

        public bool ConnectCommandCanRun(object param) => !string.IsNullOrEmpty(ServerIpAddress.Text) && CheckServerIp();


        public async void DisposeConnectionCommandRun(object param)
        {
            ServerIpAddress.IsEnabled = true;
            DeviceInfo.Status = Common.Enums.ClientConnectionStatus.Free;
            ConnectButton.IsEnabled = true;
            DisposeButton.Visibility = Visibility.Collapsed;

            BinaryWriter.Write(ConnectionRequests.DISCONNECTION_REQUEST);

            Client.Close();
            Stream.Close();
            BinaryWriter.Close();
            BinaryReader.Close();

            await ShowInfobar("Successful disconnection from the server", "Screen sharing will be suspended shortly.", InfoBarSeverity.Warning, new TimeSpan(0, 0, 3));
            ScreenShotArea.Source = new BitmapImage(new Uri(@"/images/wolf.jpg", UriKind.Relative));
        }

        public bool DisposeConnectionCommandCanRun(object param) => !string.IsNullOrEmpty(ServerIpAddress.Text);



        public void ExitCommandRun(object param) => Application.Current.Shutdown();


        public void AboutCommandRun(object param) => new About().ShowDialog();

        public void AboutDeviceCommandRun(object param) => new DeviceAbout(this.DeviceInfo).ShowDialog();

        #endregion

        #region Methods

        public InfoBarData GetBaseConnectioErrorInfoBarData()
        {
            return new InfoBarData()
            {
                Successed = false,
                Title = "There was a problem connecting to server!",
                Message = "Try again after making sure that you have typed the Server IP address correctly and that you are connected to the internet."
            };
        }

        public async void StartScreenShare()
        {
            while (true)
            {
                if (DeviceInfo.Status == Common.Enums.ClientConnectionStatus.Free)
                    return;

                RecieveScreenShot();
                await Task.Delay(41);
            }
        }

        public bool CheckInternet()
        {

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return false;

            return true;
        }

        public bool CheckServerIp()
        {
            if (!Regex.Match(ServerIpAddress.Text, @"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b").Success)
            {
                return false;
            }
            return true;
        }

        public InfoBarData ConnectServer()
        {
            // Necessary checks
            if (!CheckInternet())
                return new InfoBarData()
                {
                    Successed = false,
                    Title = "You don't have internet connection!",
                    Message = "Recheck your internet connection and try again"
                };

            if (!CheckServerIp())
                return new InfoBarData()
                {
                    Successed = false,
                    Title = "Server ip address is not correct!",
                    Message = "Please make sure you have typed the Server IP address correctly and try again."
                };


            // Initialize instances
            Client = new TcpClient();
            Client.Connect(ServerIpAddress.Text, 27001);
            Stream = Client.GetStream();
            BinaryReader = new BinaryReader(Stream);
            BinaryWriter = new BinaryWriter(Stream);
            string response = string.Empty;


            // Send connection request
            BinaryWriter.Write(ConnectionRequests.CONNECTION_REQUEST);

            // Recieve response
            response = BinaryReader.ReadString();

            // Check response
            if (!response.ToLower().Equals(ConnectionResponses.SEND_DEVICE_INFO))
                return GetBaseConnectioErrorInfoBarData();


            // Send device info
            BinaryWriter.Write(JsonSerializer.Serialize(DeviceInfo));

            // Recieve response
            response = BinaryReader.ReadString();

            // Check response
            if (!response.ToLower().Equals(ConnectionResponses.CONNECTION_SUCCESSFULLY_ESTABLISHED))
                return GetBaseConnectioErrorInfoBarData();

            return new InfoBarData() { Successed = true };

        }

        public void RecieveScreenShot()
        {
            // Initialize Instances
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] request = new byte[ushort.MaxValue];
            byte[] response = new byte[ushort.MaxValue];


            // Send request to server for get screen shot
            BinaryWriter.Write(ConnectionRequests.GET_SCREENSHOT_REQUEST);


            // Receive a responsoe for initialize remoteEP
            UdpClient.Receive(ref remoteEP);


            // Get number of parts
            response = UdpClient.Receive(ref remoteEP);
            int numberParts = int.Parse(Encoding.Default.GetString(response));
            request = Encoding.Default.GetBytes(ConnectionRequests.RECEIVED);
            UdpClient.Send(request, request.Length, remoteEP);


            // Get lenght of array
            response = UdpClient.Receive(ref remoteEP);
            int lenght = int.Parse(Encoding.Default.GetString(response));
            request = Encoding.Default.GetBytes(ConnectionRequests.RECEIVED);
            UdpClient.Send(request, request.Length, remoteEP);
            byte[] responseImage = new byte[lenght];


            // Receive screenshot
            int received = 0;
            for (int i = 0; i < numberParts; i++)
            {

                response = UdpClient.Receive(ref remoteEP);

                // Add data to responseImage
                for (int j = received, k = 0; k < response.Length; j++, k++)
                {
                    responseImage[j] = response[k];
                }

                received += response.Length;

                request = Encoding.Default.GetBytes(ConnectionRequests.RECEIVED);
                UdpClient.Send(request, request.Length, remoteEP);
            }

            // Assign image to WPF Image
            var image = ByteToImage(responseImage);
            ScreenShotArea.Source = null;
            ScreenShotArea.Source = image;
        }

        public ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg;
            return imgSrc;
        }

        public IPEndPoint GenerateEndPointFromServerIpaddress() => new IPEndPoint(IPAddress.Parse(ServerIpAddress.Text), 27001);

        public void AssigUI(ref InfoBar connectionInfobar, ref Button connectButton, ref Button disposeButton, ref TextBox serverIpAddress, ref System.Windows.Controls.Image screenShotArea)
        {
            ConnectionInfoBar = connectionInfobar;

            ConnectButton = connectButton;
            ConnectButton.Content = "Connect";
            ConnectButton.Icon = SymbolRegular.PlugDisconnected28;
            ConnectButton.Appearance = ControlAppearance.Info;

            DisposeButton = disposeButton;
            DisposeButton.Content = "Dispose Contection";
            DisposeButton.Icon = SymbolRegular.Connector24;
            DisposeButton.Appearance = ControlAppearance.Danger;
            DisposeButton.Visibility = Visibility.Collapsed;


            ServerIpAddress = serverIpAddress;

            ScreenShotArea = screenShotArea;
            ScreenShotArea.Source = new BitmapImage(new Uri(@"/images/wolf.jpg", UriKind.Relative));

        }

        public async Task ShowInfobar(string title, string message, InfoBarSeverity severity, TimeSpan openTime)
        {
            ConnectionInfoBar.Title = title;
            ConnectionInfoBar.Message = message;
            ConnectionInfoBar.Severity = severity;

            ConnectionInfoBar.IsOpen = true;
            await Task.Delay(openTime);
            ConnectionInfoBar.IsOpen = false;

        }

        public async Task ShowInfobar(InfoBarData infoBarData, InfoBarSeverity severity, TimeSpan openTime)
        {
            ConnectionInfoBar.Title = infoBarData.Title;
            ConnectionInfoBar.Message = infoBarData.Message;
            ConnectionInfoBar.Severity = severity;

            ConnectionInfoBar.IsOpen = true;
            await Task.Delay(openTime);
            ConnectionInfoBar.IsOpen = false;

        }

        #endregion

        public MainWindowVM()
        {
            ConnectCommand = new(ConnectCommandRun, ConnectCommandCanRun);
            DisposeConnectionCommand = new(DisposeConnectionCommandRun, DisposeConnectionCommandCanRun);
            ExitCommand = new(ExitCommandRun);


            AboutCommand = new(AboutCommandRun);
            AboutDeviceCommand = new(AboutDeviceCommandRun);

        }

    }
}

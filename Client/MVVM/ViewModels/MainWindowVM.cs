using Client.MVVM.Views;
using Common.Models;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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

        public UDPClient Client { get; set; }


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
            ConnectButton.IsEnabled = false;
            DisposeButton.Visibility = Visibility.Visible;
            DeviceInfo.Status = Common.Enums.ClientConnectionStatus.Connected;
            await ShowInfobar("Successfuly connected to the server", "Screen sharing will start automatically in a short time.", InfoBarSeverity.Success, new TimeSpan(0, 0, 3));

            StartScreenShare();
        }

        public bool ConnectCommandCanRun(object param) => !string.IsNullOrEmpty(ServerIpAddress.Text) && ServerIpAddress.Text.Length >= 7;


        public async void DisposeConnectionCommandRun(object param)
        {
            DeviceInfo.Status = Common.Enums.ClientConnectionStatus.Free;
            ConnectButton.IsEnabled = true;
            DisposeButton.Visibility = Visibility.Collapsed;
            
            await ShowInfobar("Successful disconnection from the server", "Screen sharing will be suspended shortly.", InfoBarSeverity.Warning, new TimeSpan(0, 0, 3));
            ScreenShotArea.Source = new BitmapImage(new Uri(@"/images/wolf.jpg",UriKind.Relative));
        }

        public bool DisposeConnectionCommandCanRun(object param) => !string.IsNullOrEmpty(ServerIpAddress.Text);



        public void ExitCommandRun(object param) => Application.Current.Shutdown();


        public void AboutCommandRun(object param) => new About().ShowDialog();

        public void AboutDeviceCommandRun(object param) => new DeviceAbout(this.DeviceInfo).ShowDialog();

        #endregion

        #region Methods

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

        private void RecieveScreenShot()
        {
            var client = new Socket(
                                   AddressFamily.InterNetwork,
                                   SocketType.Dgram,
                                   ProtocolType.Udp
                                   );

            byte[] buffer = new byte[ushort.MaxValue];
            EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 27001);

            client.SendTo(Encoding.Default.GetBytes("get"), SocketFlags.None, endPoint);


            var len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            string response = Encoding.Default.GetString(buffer, 0, len);

            // Check response
            if (response.ToLower() != "start")
                return;


            // Get number of parts
            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int numberParts = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            // Get lenght of array
            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int lenght = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);
            byte[] responseImage = new byte[lenght];

            // Recive screenshot
            int received = 0;
            for (int i = 0; i < numberParts; i++)
            {

                len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

                // Add data to responseImage
                for (int j = received, k = 0; k < len; j++, k++)
                {
                    responseImage[j] = buffer[k];
                }

                client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

                received += len;
            }

            // Convert Bytes

            // Assign image to WPF Image
            var image = ByteToImage(responseImage);
            ScreenShotArea.Source = image;
        }

        public ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg as ImageSource;
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

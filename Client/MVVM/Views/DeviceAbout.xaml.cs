using Common.Models;
using System.Windows;
using System.Windows.Input;

namespace Client.MVVM.Views
{
    /// <summary>
    /// Interaction logic for DeviceAbout.xaml
    /// </summary>
    public partial class DeviceAbout : Window
    {
        public DeviceAbout(DeviceInfo deviceInfo)
        {
            InitializeComponent();
            DataContext = deviceInfo;
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


    }
}

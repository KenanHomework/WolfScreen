using System.Windows;
using System.Windows.Input;

namespace Client.MVVM.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            InfoText.Text = "This project provides real time screen sharing \nbetween 2 devices with\n UDP protocol over Socket in a simple way.";
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

    }
}

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Minecraft_Server_Wrapper
{
    /// <summary>
    /// Interaction logic for ServerStartSettings.xaml
    /// </summary>
    public partial class ServerStartSettings : Window
    {
        public DateTime AutoStartTime;
        public DateTime AutoStopTime;
        public bool AutoStartTimeBased = false;

        public ServerStartSettings()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Hide();

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            }
            catch (Exception)
            {

            }
        }

        

        private void StartTimeDate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DateTime.TryParse(StartTimeDate.Text, out DateTime StartTimeParsed))
            {
                StartTimeDate.Width = 280;
                StartTimeErrorSign.Opacity = 0;
                StartTimeDate.Foreground = new SolidColorBrush(Colors.Black);
                AutoStartTime = DateTime.Parse(StartTimeDate.Text);
            }
            if (!DateTime.TryParse(StartTimeDate.Text, out DateTime StartTimeParseFail))
            {
                StartTimeErrorSign.Opacity = 1;
                StartTimeDate.Width = 255;
                StartTimeDate.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void StopTimeDate_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DateTime.TryParse(StopTimeDate.Text, out DateTime StopTimeParsed))
            {
                StopTimeErrorSign.Opacity = 0;
                StopTimeDate.Width = 280;
                StopTimeDate.Foreground = new SolidColorBrush(Colors.Black);
                AutoStopTime = DateTime.Parse(StopTimeDate.Text);
            }
            if (!DateTime.TryParse(StopTimeDate.Text, out DateTime StopTimeParseFail))
            {
                StopTimeErrorSign.Opacity = 1;
                StopTimeDate.Width = 255;
                StopTimeDate.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => AutoStartTimeBased = true;

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => AutoStartTimeBased = false;
    }
}

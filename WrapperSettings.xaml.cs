using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Minecraft_Server_Wrapper
{
    /// <summary>
    /// Interaction logic for WrapperSettings.xaml
    /// </summary>
    public partial class WrapperSettings : Window
    {

        ServerWrapper serverWrapper = new ServerWrapper();
        WinForms.ColorDialog colorDialog = new WinForms.ColorDialog();

        public WrapperSettings()
        {
            InitializeComponent();
            TitleBarColor.Fill = new SolidColorBrush(Color.FromRgb(serverWrapper.TitleBarColor.R, serverWrapper.TitleBarColor.G, serverWrapper.TitleBarColor.B));
            WarningOutputColor.Fill = new SolidColorBrush(Color.FromRgb(serverWrapper.WarningOutputColor.R, serverWrapper.WarningOutputColor.G, serverWrapper.WarningOutputColor.B));
            ErrorOutputColor.Fill = new SolidColorBrush(Color.FromRgb(serverWrapper.ErrorOutputColor.R, serverWrapper.ErrorOutputColor.G, serverWrapper.ErrorOutputColor.B));
            if (!serverWrapper.ShowWarningOutput)
            {
                WarningOutputColor.Opacity = 0.1;
                WarningOutputColorLabel.Opacity = 0.1;
                WarningOutputColor.IsEnabled = false;
                serverWrapper.ShowWarningOutput = false;
            }
            if (!serverWrapper.ShowErrorOutput)
            {
                ErrorOutputColor.Opacity = 0.1;
                ErrorOutputColorLabel.Opacity = 0.1;
                ErrorOutputColor.IsEnabled = false;
                serverWrapper.ShowErrorOutput = false;
            }
            try
            {
                ShowWarningOutput.IsChecked = serverWrapper.ShowWarningOutput;
                ShowErrorOutput.IsChecked = serverWrapper.ShowErrorOutput;
            }
            catch (Exception)
            {
            }

            if (File.Exists(serverWrapper.BackgroundSkin))
            {
                ChangeBackgroundSkin.Content = Path.GetFileName(serverWrapper.BackgroundSkin);
            }
            else
            {
                ChangeBackgroundSkin.Content = "No Skin";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

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

        private void TitleBarColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                
                TitleBarColor.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                serverWrapper.TitleBarColor = colorDialog.Color;
                serverWrapper.Save();
            }
        }

        private void DefaultOutputColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                DefaultOutputColor.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                serverWrapper.DefaultOutputColor = colorDialog.Color;
                serverWrapper.Save();
            }
        }

        private void WarningOutputColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                WarningOutputColor.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                serverWrapper.WarningOutputColor = colorDialog.Color;
                serverWrapper.Save();
            }
        }

        private void ErrorOutputColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                ErrorOutputColor.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                serverWrapper.ErrorOutputColor = colorDialog.Color;
                serverWrapper.Save();
            }
        }

        private void ShowWarningOutput_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //WarningOutputColor.Opacity = 1;
                //WarningOutputColorLabel.Opacity = 1;
                //WarningOutputColor.IsEnabled = true;
                serverWrapper.ShowWarningOutput = true;
                serverWrapper.Save();
            }
            catch (Exception)
            {
            }
        }

        private void ShowWarningOutput_Unchecked(object sender, RoutedEventArgs e)
        {
            WarningOutputColor.Opacity = 0.1;
            WarningOutputColorLabel.Opacity = 0.1;
            WarningOutputColor.IsEnabled = false;
            serverWrapper.ShowWarningOutput = false;
            serverWrapper.Save();
        }

        private void ShowErrorOutput_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //ErrorOutputColor.Opacity = 1;
                //ErrorOutputColorLabel.Opacity = 1;
                //ErrorOutputColor.IsEnabled = true;
                serverWrapper.ShowErrorOutput = true;
                serverWrapper.Save();
            }
            catch (Exception)
            {
            }
        }

        private void ShowErrorOutput_Unchecked(object sender, RoutedEventArgs e)
        {
            ErrorOutputColor.Opacity = 0.1;
            ErrorOutputColorLabel.Opacity = 0.1;
            ErrorOutputColor.IsEnabled = false;
            serverWrapper.ShowErrorOutput = false;
            serverWrapper.Save();
        }

        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void ForegroundOpacityValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ForegroundOpacityValue.Text != null)
            {
                if (IsTextAllowed(ForegroundOpacityValue.Text) && Convert.ToInt32(ForegroundOpacityValue.Text) >= 0 && Convert.ToInt32(ForegroundOpacityValue.Text) <= 100)
                {
                    if (Convert.ToInt32(ForegroundOpacityValue.Text) != 0)
                    {
                        //new MainWindow().ServerOutputWindow.Opacity = Convert.ToInt32(ForegroundOpacityValue.Text) / 100;
                        serverWrapper.ForegroundOpacity = Convert.ToInt32(ForegroundOpacityValue.Text) / 100;
                    }
                    if (Convert.ToInt32(ForegroundOpacityValue.Text) == 0)
                    {
                        //new MainWindow().ServerOutputWindow.Opacity = 0;
                        serverWrapper.ForegroundOpacity = 0;
                    }
                }

                if (!IsTextAllowed(ForegroundOpacityValue.Text))
                {
                    ForegroundOpacityValue.Text = Regex.Replace(ForegroundOpacityValue.Text, "[^0-9]", "");
                }
            }
        }

        private void ChangeBackgroundSkin_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                serverWrapper.BackgroundSkin = "";
                serverWrapper.Save();
                ChangeBackgroundSkin.Content = "No Skin";
            }
            else
            {
                WinForms.OpenFileDialog BackgroundSkinFile = new WinForms.OpenFileDialog();
                BackgroundSkinFile.Filter = "Image or Video (.png .jpg .jpeg .bmp .mp4)|*.png;*.jpg;*.jpeg;*.bmp;*.mp4";

                if (BackgroundSkinFile.ShowDialog() == WinForms.DialogResult.OK)
                {
                    serverWrapper.BackgroundSkin = BackgroundSkinFile.FileName;
                    serverWrapper.Save();
                    ChangeBackgroundSkin.Content = Path.GetFileName(BackgroundSkinFile.FileName);
                    new MainWindow().UpdateSkin(Convert.ToSingle(ForegroundOpacityValue.Text), serverWrapper.BackgroundSkin);
                }
            }
        }
    }
}

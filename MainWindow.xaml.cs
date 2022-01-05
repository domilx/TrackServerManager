using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.IO.Compression;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace Minecraft_Server_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        string JarPath;
        ServerWrapper serverWrapper = new ServerWrapper();
        //ServerPropertiesManager serverPropertiesManager = new ServerPropertiesManager();
        WrapperSettings wrapperSettings = new WrapperSettings();
        FileSystemWatcher ModsPluginsWatcher = new FileSystemWatcher();
        FileSystemWatcher WorldsWatcher = new FileSystemWatcher();

        Color DefaultOutputColor;
        Color WarningOutputColor;
        Color ErrorOutputColor;
        Color PlayerEventOutputColor;
        Color ServerDoneLoadingColor;

        public MainWindow()
        {
            InitializeComponent();

            try { PublicIP.Content = new WebClient().DownloadString(new Uri("http://ipinfo.io/ip")); } catch { PublicIP.Content = "Unable to get IP"; }

            ramLimit.Text = serverWrapper.ServerRAM.ToString();
            ServerFilePath.Text = serverWrapper.ServerPath;
            //----------BUG: Force Online Mode prevents app from starting outside of debug folder----------
            /*
            if (serverWrapper.ServerForceOnlineMode)
            {
                ForceOnlineMode.IsChecked = serverWrapper.ServerForceOnlineMode;
                //VisualStateManager.GoToState(ForceOnlineMode, "CheckBoxChecked", true);
            }
            if (!serverWrapper.RunServerOnStartUp)
            {
                RunServerOnStartUp.IsChecked = serverWrapper.RunServerOnStartUp;
                //VisualStateManager.GoToState(RunServerOnStartUp, "CheckBoxChecked", true);
            }
            */

            ServerOutputWindow_AutoScrollButton.Opacity = 0;
            TitleBar.Background = new SolidColorBrush(Color.FromRgb(serverWrapper.TitleBarColor.R, serverWrapper.TitleBarColor.G, serverWrapper.TitleBarColor.B));
            //serverPropertiesManager.TitleBar.Background = new SolidColorBrush(Color.FromRgb(serverWrapper.TitleBarColor.R, serverWrapper.TitleBarColor.G, serverWrapper.TitleBarColor.B));
            wrapperSettings.TitleBar.Background = new SolidColorBrush(Color.FromRgb(serverWrapper.TitleBarColor.R, serverWrapper.TitleBarColor.G, serverWrapper.TitleBarColor.B));
            DefaultOutputColor = Color.FromRgb(serverWrapper.DefaultOutputColor.R, serverWrapper.DefaultOutputColor.G, serverWrapper.DefaultOutputColor.B);
            WarningOutputColor = Color.FromRgb(serverWrapper.WarningOutputColor.R, serverWrapper.WarningOutputColor.G, serverWrapper.WarningOutputColor.B);
            ErrorOutputColor = Color.FromRgb(serverWrapper.ErrorOutputColor.R, serverWrapper.ErrorOutputColor.G, serverWrapper.ErrorOutputColor.B);
            PlayerEventOutputColor = Color.FromRgb(serverWrapper.PlayerEventOutputColor.R, serverWrapper.PlayerEventOutputColor.G, serverWrapper.PlayerEventOutputColor.B);
            ServerDoneLoadingColor = Color.FromRgb(serverWrapper.ServerLoadingDoneColor.R, serverWrapper.ServerLoadingDoneColor.G, serverWrapper.ServerLoadingDoneColor.B);
            NotesRichTextBox.AppendText(serverWrapper.Notes);

            //Does server path exist and is auto start available
            if (File.Exists(ServerFilePath.Text))
            {
                ServerCheck("ServerPath");
                StatusLightColor(1);
            }
            if (ServerFilePath.Text == "...\\server.jar")
            {
                ShowInExplorer.IsEnabled = false;
                StartStopServer.IsEnabled = false;
                StatusIndicator.Content = "No server selected";
                StatusLightColor(0);
            }
            if (!File.Exists(ServerFilePath.Text) && ServerFilePath.Text != "...\\server.jar")
            {
                StatusIndicator.Content = "Could not find server file at last known path";
                StatusLightColor(0);
            }
            if (File.Exists(ServerFilePath.Text) && serverWrapper.RunServerOnStartUp == true)
            {
                OnServerStart();
            }

            //Skinning [wip]
            UpdateSkin(0.95f, serverWrapper.BackgroundSkin);

            ModsPluginsWatcher.Changed += ModsPlugins_OnChanged;
            ModsPluginsWatcher.Created += ModsPlugins_OnChanged;
            ModsPluginsWatcher.Deleted += ModsPlugins_OnChanged;
            ModsPluginsWatcher.Renamed += ModsPlugins_OnChanged;

            //if (ModsPluginsWatcher.Path != null) { ModsPluginsWatcher.EnableRaisingEvents = true; }

            WorldsWatcher.Changed += Worlds_OnChanged;
            WorldsWatcher.Created += Worlds_OnChanged;
            WorldsWatcher.Deleted += Worlds_OnChanged;
            WorldsWatcher.Renamed += Worlds_OnChanged;

            //if (WorldsWatcher.Path != null) { WorldsWatcher.EnableRaisingEvents = true; }
        }

        private void Worlds_OnChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => {
                WorldsTabListBox.Items.Clear();
                foreach (var folder in Directory.EnumerateDirectories(WorkingDirectory.ToString()))
                {
                    if (File.Exists(folder + @"\level.dat"))
                    {
                        string[] temp = folder.Split('\\');
                        WorldsTabListBox.Items.Add(temp[temp.Length - 1]);
                    }
                }
            });
        }

        private void ModsPlugins_OnChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => {
                ModPluginWindow.Items.Clear();
                if (Directory.Exists(WorkingDirectory + @"\plugins"))
                {
                    ModPluginsTabItem.Header = "Plugins";
                    string[] _Plugins = Directory.GetFiles(WorkingDirectory + @"\plugins");
                    foreach (var item in _Plugins)
                    {
                        ModPluginWindow.Items.Add(Path.GetFileNameWithoutExtension(item));
                    }
                }

                if (Directory.Exists(WorkingDirectory + @"\mods"))
                {
                    ModPluginsTabItem.Header = "Mods";
                    string[] _Mods = Directory.GetFiles(WorkingDirectory + @"\mods");
                    foreach (var item in _Mods)
                    {
                        ModPluginWindow.Items.Add(Path.GetFileNameWithoutExtension(item));
                    }
                }
            });
        }

        public void UpdateSkin(float Opacity, string BG_Path)
        {
            if (File.Exists(BG_Path))
            {

                switch (Path.GetExtension(serverWrapper.BackgroundSkin))
                {
                    case ".png":
                        GridWindow.Background = new ImageBrush(new BitmapImage(new Uri(serverWrapper.BackgroundSkin)));
                        break;

                    case ".jpg":
                        GridWindow.Background = new ImageBrush(new BitmapImage(new Uri(serverWrapper.BackgroundSkin)));
                        break;

                    case ".jpeg":
                        GridWindow.Background = new ImageBrush(new BitmapImage(new Uri(serverWrapper.BackgroundSkin)));
                        break;

                    case ".bmp":
                        GridWindow.Background = new ImageBrush(new BitmapImage(new Uri(serverWrapper.BackgroundSkin)));
                        break;

                    case ".mp4":

                        break;
                }
            }
        }

        private void PublicIP_MouseUp(object sender, MouseButtonEventArgs e) => Clipboard.SetText(PublicIP.Content.ToString(), TextDataFormat.UnicodeText);

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ServerIsRunning == true)
            {
                ServerProcess.OutputDataReceived -= new DataReceivedEventHandler(ServerOutput_OutputDataRecieved);
                ServerProcess.Exited -= new EventHandler(ServerClose_Exited);
                ServerProcess.Kill();
            }

            ModsPluginsWatcher.Changed -= ModsPlugins_OnChanged;
            ModsPluginsWatcher.Created -= ModsPlugins_OnChanged;
            ModsPluginsWatcher.Deleted -= ModsPlugins_OnChanged;
            ModsPluginsWatcher.Renamed -= ModsPlugins_OnChanged;

            Application.Current.Shutdown();
        }

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

        private void WrapperSettings_Click(object sender, RoutedEventArgs e) => new WrapperSettings().Show();

        private void StatusLightColor(int x)
        {
            if (x == 0) { StatusLight.Fill = new SolidColorBrush(Colors.Red); }
            if (x == 1) { StatusLight.Fill = new SolidColorBrush(Colors.Orange); }
            if (x == 2) { StatusLight.Fill = new SolidColorBrush(Colors.Lime); }
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);

        DirectoryInfo WorkingDirectory;
        private void ServerCheck(string changedValue)
        {
            bool ramLimitTestPass = false;
            bool ServerPathExistsTestPass = false;
            try
            {
                //Check if entered RAM is an Integer
                if (IsTextAllowed(ramLimit.Text))
                {
                    try
                    {
                        ramLimitTestPass = true;
                        if (changedValue == "ramLimit")
                        {
                            ramLimit.Foreground = new SolidColorBrush(Colors.Black);
                            StatusIndicator.Content = "RAM Limit changed to " + ramLimit.Text + "MB";
                            serverWrapper.ServerRAM = Convert.ToInt32(ramLimit.Text);
                            serverWrapper.Save();
                        }

                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    ramLimitTestPass = false;
                    if (changedValue == "ramLimit")
                    {
                        ramLimit.Foreground = new SolidColorBrush(Colors.Red);
                        StatusIndicator.Content = "RAM Limit must be an integer!";
                    }
                }

                //Check that given server file path actually exists
                if (File.Exists(ServerFilePath.Text))
                {
                    ServerPathExistsTestPass = true;
                    if (changedValue == "ServerPath")
                    {
                        WorkingDirectory = new DirectoryInfo(Path.GetDirectoryName(ServerFilePath.Text)); //Set Working Directory
                        WorldsTabListBox.Items.Clear(); //Reset Worlds List
                        WorldsWatcher.Path = WorkingDirectory.ToString(); //Set FileSystemWatcher Path
                        if (!WorldsWatcher.EnableRaisingEvents) { WorldsWatcher.EnableRaisingEvents = true; } //Enable FileSystemWatcher if disabled
                        foreach (var folder in Directory.EnumerateDirectories(WorkingDirectory.ToString()))
                        {
                            if (File.Exists(folder + @"\level.dat"))
                            {
                                string[] temp = folder.Split('\\');
                                WorldsTabListBox.Items.Add(temp[temp.Length - 1]);
                            }
                        }
                        StatusIndicator.Content = "Server path changed";
                        ShowInExplorer.IsEnabled = true;
                        BackupWorld.IsEnabled = true;

                        //Enable Properties Editor if server.properties exists
                        if (File.Exists(WorkingDirectory + @"\server.properties"))
                        {
                            EditServerProperties.IsEnabled = true;
                            //serverPropertiesManager.ServerPropertiesValues = File.ReadAllLines(WorkingDirectory + @"\server.properties");
                            //serverPropertiesManager.LoadSettings();
                        }
                        serverWrapper.ServerPath = ServerFilePath.Text;
                        serverWrapper.Save();

                        //Check is server is using mods or plugins and display those mods/plugins
                        ModPluginWindow.Items.Clear();
                        if (Directory.Exists(WorkingDirectory + @"\plugins"))
                        {
                            ModPluginsTabItem.Header = "Plugins"; //Set Tab Header
                            ModsPluginsWatcher.Path = WorkingDirectory + @"\plugins"; //Set FileSystemWatcher Path
                            if (!ModsPluginsWatcher.EnableRaisingEvents) { ModsPluginsWatcher.EnableRaisingEvents = true; } //Enable FileSystemWatcher if disabled
                            string[] _Plugins = Directory.GetFiles(WorkingDirectory + @"\plugins");
                            int i = 0;
                            foreach (var item in _Plugins) {ModPluginWindow.Items.Add(Path.GetFileNameWithoutExtension(item)); i++; }
                            ModPluginCount.Content = i;
                        }

                        if (Directory.Exists(WorkingDirectory + @"\mods"))
                        {
                            ModPluginsTabItem.Header = "Mods"; //Set Tab Header
                            ModsPluginsWatcher.Path = WorkingDirectory + @"\mods"; //Set FileSystemWatcher Path
                            if (!ModsPluginsWatcher.EnableRaisingEvents) { ModsPluginsWatcher.EnableRaisingEvents = true; } //Enable FileSystemWatcher if disabled
                            string[] _Mods = Directory.GetFiles(WorkingDirectory + @"\mods");
                            int i = 0;
                            foreach (var item in _Mods) { ModPluginWindow.Items.Add(Path.GetFileNameWithoutExtension(item)); i++; }
                            ModPluginCount.Content = i;
                        }
                    }
                }
                else
                {
                    try
                    {
                        ServerPathExistsTestPass = false;
                        if (changedValue == "ServerPath")
                        {
                            StatusIndicator.Content = "Server path is invalid!";
                            ShowInExplorer.IsEnabled = false;
                            BackupWorld.IsEnabled = false;
                            EditServerProperties.IsEnabled = false;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                //Is server ready to start
                if (ramLimitTestPass && ServerPathExistsTestPass)
                {
                    StartStopServerSettings.IsEnabled = true;
                    StartStopServer.IsEnabled = true;
                    StatusLightColor(1);
                }
                else
                {
                    StartStopServerSettings.IsEnabled = false;
                    StartStopServer.IsEnabled = false;
                    StatusLightColor(0);
                }
            }
            catch (Exception)
            {

            }
        }

        //Main Code
        bool ServerIsRunning = false;

        //Auto Start Server Settings
        private void RunServerOnStartUp_Checked(object sender, RoutedEventArgs e) => serverWrapper.RunServerOnStartUp = true;

        private void RunServerOnStartUp_Unchecked(object sender, RoutedEventArgs e) => serverWrapper.RunServerOnStartUp = false;

        //Managing Server Path
        private void BrowseServerFile_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog BrowseServerFileWindow = new WinForms.OpenFileDialog();
            BrowseServerFileWindow.Filter = ".jar|*.jar";

            if (BrowseServerFileWindow.ShowDialog() == WinForms.DialogResult.OK)
            {
                ServerFilePath.Text = BrowseServerFileWindow.FileName;
                StatusIndicator.Content = "Server path changed";
                StatusLightColor(1);
            }

            ServerCheck("ServerPath");
        }

        //Make new Server
        //private void CreateNewServer_Click(object sender, RoutedEventArgs e) => new CreateNewServer().Show();

        //Setting Server Path
        private void ServerFilePath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ServerCheck("ServerPath");

        private void ServerFilePath_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed)
            {
                ServerFilePath.Clear();
                ServerCheck("ServerPath");
            }
        }

        //Run Server Settings
        private void StartStopServerSettings_Click(object sender, RoutedEventArgs e) => new ServerStartSettings().Show();

        //Managing Server Mods/Plugins
        private void PluginsModItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(WorkingDirectory + @"\mods"))
            {
                File.Delete(WorkingDirectory + @"\mods\" + ModPluginWindow.SelectedItem.ToString() + ".jar");
            }
            if (Directory.Exists(WorkingDirectory + @"\plugins"))
            {
                File.Delete(WorkingDirectory + @"\plugins\" + ModPluginWindow.SelectedItem.ToString() + ".jar");
            }
        }

        private void PluginsModItemOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(WorkingDirectory + @"\mods"))
            {
                Process.Start("explorer.exe", "/select, \"" + WorkingDirectory + @"\mods\" + ModPluginWindow.SelectedItem.ToString() + ".jar\"");
            }
            if (Directory.Exists(WorkingDirectory + @"\plugins"))
            {
                Process.Start("explorer.exe", "/select, \"" + WorkingDirectory + @"\plugins\" + ModPluginWindow.SelectedItem.ToString() + ".jar\"");
            }
        }

        //World Tab Options
        private async void Worlds_Action(object sender, RoutedEventArgs e)
        {
            //Backup Specific World
            if (sender == Worlds_Backup)
            {
                //Check backups directory exists
                if (!Directory.Exists(WorkingDirectory + @"\Backups"))
                {
                    Directory.CreateDirectory(WorkingDirectory + @"\Backups");
                }

                //Create Folder to hold backups of current time
                string DestinationFolder = WorkingDirectory + "\\Backups\\" + DateTime.Now.ToString("dd-mmmm-yyyy-_-HH-mm-ss");
                Directory.CreateDirectory(DestinationFolder);

                //Find all world folders
                await Task.Run(() => {
                    Dispatcher.Invoke(() =>
                    {
                        Directory.CreateDirectory(DestinationFolder + "\\" + WorldsTabListBox.SelectedItem.ToString());
                        ServerOutputWindow.AppendText("\n[Server Wrapper] Backing up world \"" + WorldsTabListBox.SelectedItem.ToString() + "\"");
                        try
                        {
                            DirectoryCopy(WorkingDirectory + "\\" + WorldsTabListBox.SelectedItem.ToString(), DestinationFolder + "\\" + WorldsTabListBox.SelectedItem.ToString(), true);
                            ServerOutputWindow.AppendText("\n[Server Wrapper] Finished backing up world \"" + WorldsTabListBox.SelectedItem.ToString() + "\"");
                        }
                        catch (Exception f)
                        {
                            ServerOutputWindow.AppendText(f.ToString());
                            ServerOutputWindow.AppendText("\n[Server Wrapper] Failed to backup world \"" + WorldsTabListBox.SelectedItem.ToString() + "\"");
                        }
                    });
                });
            }

            //Open World in Explorer
            if (sender == Worlds_OpenInExplorer) { Process.Start("explorer.exe", WorkingDirectory + "\\" + WorldsTabListBox.SelectedItem.ToString()); }

            //Delete World
            if (sender == Worlds_Delete) { Directory.Delete(WorkingDirectory + "\\" + WorldsTabListBox.SelectedItem.ToString()); }
        }

        //Managing Server Memory
        private void ramLimit_TextChanged(object sender, TextChangedEventArgs e) => ServerCheck("ramLimit");

        private void ramLimit_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) + 1).ToString();
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) + 128).ToString();
                }
                else
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) + 1024).ToString();
                }
            }
            if (e.Delta < 0)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) - 1).ToString();
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) - 128).ToString();
                }
                else
                {
                    ramLimit.Text = (Convert.ToInt32(ramLimit.Text) - 1024).ToString();
                }

                if (Convert.ToInt32(ramLimit.Text) < 0)
                {
                    ramLimit.Text = "0";
                }
            }
        }

        //Show Server in Explorer
        private void ShowInExplorer_Click(object sender, RoutedEventArgs e)
        {
            string EULA = Path.GetDirectoryName(ServerFilePath.Text) + "\\eula.txt";
            string text = File.ReadAllText(EULA);
            text = text.Replace("false", "true");
            File.WriteAllText(EULA, text);
            System.Windows.Forms.MessageBox.Show("Eula is now set to true","Eula");

        }

        //Backup Server World
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private async void BackupWorld_Click(object sender, RoutedEventArgs e)
        {
            BackupWorld.IsEnabled = false;
            BackupWorld.Content = "Backing up Worlds";
            ServerOutputWindow.AppendText("\n[Server Wrapper] Starting Backup");

            //Check backups directory exists
            if (!Directory.Exists(WorkingDirectory + @"\Backups"))
            {
                Directory.CreateDirectory(WorkingDirectory + @"\Backups");
            }

            //Create Folder to hold backups of current time
            string DestinationFolder = WorkingDirectory + "\\Backups\\" + DateTime.Now.ToString("dd-mm-yyyy-_-HH-mm-ss");
            Directory.CreateDirectory(DestinationFolder);

            //Find all world folders and copy
            await Task.Run(() => {
                foreach (var item in Directory.EnumerateDirectories(WorkingDirectory.ToString()))
                {
                    if (File.Exists(item + "\\level.dat"))
                    {
                        string[] WorldName = item.Split('\\');
                        Directory.CreateDirectory(DestinationFolder + "\\" + WorldName[WorldName.Length - 1]);
                        Dispatcher.Invoke(() => { ServerOutputWindow.AppendText("\n[Server Wrapper] Backing up world \"" + WorldName[WorldName.Length - 1] + "\""); });
                        try
                        {
                            DirectoryCopy(item, DestinationFolder + "\\" + WorldName[WorldName.Length - 1], true);
                        }
                        catch (Exception f)
                        {
                            ServerOutputWindow.AppendText(f.ToString());
                        }
                        Dispatcher.Invoke(() => { ServerOutputWindow.AppendText("\n[Server Wrapper] Finished backing up world \"" + WorldName[WorldName.Length - 1] + "\""); });
                    }
                }
            });

            ServerOutputWindow.AppendText("\n[Server Wrapper] Backuping Worlds Complete");
            BackupWorld.Content = "Backup Server World(s)";
            BackupWorld.IsEnabled = true;
        }

        //Clear Server Output
        private void ClearOutputWindow_Click(object sender, RoutedEventArgs e)
        {
            ServerOutputWindow.Document.Blocks.Clear();
        }

        //Editing Server Properties File
        private void EditServerProperties_Click(object sender, RoutedEventArgs e)
        {
            //serverPropertiesManager.Show();
            Process.Start(Path.GetDirectoryName(ServerFilePath.Text));
        }

        

        //Running Server
        ProcessStartInfo ServerArgs;
        Process ServerProcess = new Process();

        //Force Online mode toggle
        private void ForceOnlineMode_Checked(object sender, RoutedEventArgs e)
        {
            ServerArgs = new ProcessStartInfo("java", "-Xmx" + ramLimit.Text + "M -jar \"" + ServerFilePath.Text + "\" nogui -o");
            StatusIndicator.Content = "Force online mode enabled";
            
            ServerArgs.RedirectStandardInput = true;
            ServerArgs.RedirectStandardOutput = true;
            ServerArgs.UseShellExecute = false;
            ServerArgs.CreateNoWindow = true;

            serverWrapper.ServerForceOnlineMode = true;
        }

        private void ForceOnlineMode_Unchecked(object sender, RoutedEventArgs e)
        { 
            ServerArgs = new ProcessStartInfo("java", "-Xmx" + ramLimit.Text + "M -jar \"" + ServerFilePath.Text + "\" nogui");
            StatusIndicator.Content = "Force online mode disabled";

            ServerArgs.RedirectStandardInput = true;
            ServerArgs.RedirectStandardOutput = true;
            ServerArgs.UseShellExecute = false;
            ServerArgs.CreateNoWindow = true;

            serverWrapper.ServerForceOnlineMode = false;
        }

        Stopwatch ServerUptime = new Stopwatch();
        //RAM and CPU usage and Server Uptime
        private void UpdateCpuRamUsageTimer(bool StartTimer)
        {
            Timer timer = new Timer(); //For updating RAM and CPU usage
            timer.Interval = 100;
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(UpdateCpuRamUsage_ElapsedEventHandler);

            Timer UpTimeTick = new Timer();
            UpTimeTick.Interval = 1000;
            UpTimeTick.AutoReset = true;
            UpTimeTick.Elapsed += new ElapsedEventHandler(UpdateServerUptime);
            if (StartTimer)
            {
                timer.Start();
                UpTimeTick.Start();
                ServerUptime.Start();
            }
            if (!StartTimer)
            {
                timer.Stop();
                UpTimeTick.Stop();
                ServerUptime.Stop();
                ServerUptime.Reset();

                ramUsageValue.Content = "RAM Usage: ----MB";
            }
        }

        private void UpdateServerUptime(object sender, ElapsedEventArgs e)
        {
            if (ServerIsRunning)
            {
                Dispatcher.Invoke(() => { StatusIndicator.Content = "Server is Running | Server Process ID: " + ServerProcess.Id + " | Server Uptime: " + ServerUptime.Elapsed.ToString("hh\\:mm\\:ss"); });
            }
        }

        private void UpdateCpuRamUsage_ElapsedEventHandler(object sender, EventArgs e)
        {
            if (ServerIsRunning)
            {
                Dispatcher.Invoke(() => {
                    ServerProcess.Refresh();
                    long RamUsedMB = ServerProcess.WorkingSet64 / (1024 * 1024);
                    if (RamUsedMB / int.Parse(ramLimit.Text) >= 0.9) { ramUsageValue.Foreground = new SolidColorBrush(Colors.Red); }
                    else if (RamUsedMB / int.Parse(ramLimit.Text) < 0.9 && RamUsedMB / int.Parse(ramLimit.Text) >= 0.7) { ramUsageValue.Foreground = new SolidColorBrush(Colors.Yellow); }
                    else { ramUsageValue.Foreground = new SolidColorBrush(Colors.White); }
                    ramUsageValue.Content = "RAM Usage: " + RamUsedMB + "MB";
                });
            }
        }

        //On server start and stop
        private void OnServerStart()
        {
            //Check that server arguements are valid
            if (ServerArgs == null)
            {
                ServerProcess.StartInfo.WorkingDirectory = WorkingDirectory.ToString();
                ServerOutputWindow.AppendText("java -Xmx" + ramLimit.Text + "M -jar \"" + ServerFilePath.Text + "\" nogui\n");
                ServerArgs = new ProcessStartInfo("java", "-Xmx" + ramLimit.Text + "M -jar \"" + ServerFilePath.Text + "\" nogui");
                ServerArgs.RedirectStandardInput = true;
                ServerArgs.RedirectStandardOutput = true;
                ServerArgs.UseShellExecute = false;
                ServerArgs.CreateNoWindow = true;
            }

            //Start Server
            ServerProcess.StartInfo = ServerArgs;
            ServerProcess.StartInfo.WorkingDirectory = WorkingDirectory.ToString();
            ServerProcess.EnableRaisingEvents = true;
            ServerProcess.OutputDataReceived += new DataReceivedEventHandler(ServerOutput_OutputDataRecieved);
            ServerProcess.Exited += new EventHandler(ServerClose_Exited);
            ServerProcess.Start();
            ServerProcess.BeginOutputReadLine();

            ServerIsRunning = true;
            
            BrowseServerFile.IsEnabled = false;
            ServerFilePath.IsEnabled = false;
            ForceOnlineMode.IsChecked = false;
            ramLimit.IsEnabled = false;
            //KickAll.IsEnabled = true;
            //BanAll.IsEnabled = true;
            //opAll.IsEnabled = true;
            //deopAll.IsEnabled = true;
            SendCommand.IsEnabled = true;

            StartStopServer.ToolTip = "Shift Click to Force Stop Server";
            StartStopServer.Content = "Stop Server";

            StatusIndicator.Content = "Server is Running | Server Process ID: " + ServerProcess.Id;
            StatusLightColor(2);

            UpdateCpuRamUsageTimer(true);
        }

        private void OnServerStop(bool ServerForceClose)
        {
            UpdateCpuRamUsageTimer(false);
            ServerIsRunning = false;
            ServerProcess.CancelOutputRead();
            BrowseServerFile.IsEnabled = true;
            ServerFilePath.IsEnabled = true;
            ForceOnlineMode.IsEnabled = true;
            ramLimit.IsEnabled = true;
            //KickAll.IsEnabled = false;
            //BanAll.IsEnabled = false;
            //opAll.IsEnabled = false;
            //deopAll.IsEnabled = false;
            SendCommand.IsEnabled = false;

            if (ServerForceClose) { ServerOutputWindow.AppendText("\nServer Force Closed"); }

            if (!ServerForceClose && ServerProcess.HasExited) { ServerOutputWindow.AppendText("\nServer Closed"); }

            StartStopServer.IsEnabled = true;
            StartStopServer.Content = "Start Server";
        }

        //Start Server Handler
        private void StartStopServer_Click(object sender, RoutedEventArgs e)
        {
            if (!ServerIsRunning)
            {
                OnServerStart();
            }
            else
            {
                //Force Close Server
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    UpdateCpuRamUsageTimer(false);
                    ServerProcess.OutputDataReceived -= new DataReceivedEventHandler(ServerOutput_OutputDataRecieved);
                    ServerProcess.Exited -= new EventHandler(ServerClose_Exited);
                    ServerProcess.Kill();

                    OnServerStop(true);
                    StatusIndicator.Content = "Server Force Closed";
                    StatusLightColor(1);
                }
                else
                {
                    //Stop Server
                    StartStopServer.IsEnabled = false;
                    StartStopServer.Content = "Stopping Server";
                    ServerProcess.StandardInput.WriteLine("stop");
                    UpdateCpuRamUsageTimer(false);
                    ServerProcess.OutputDataReceived -= new DataReceivedEventHandler(ServerOutput_OutputDataRecieved);
                    ServerProcess.Exited -= new EventHandler(ServerClose_Exited);

                    OnServerStop(false);
                    StatusIndicator.Content = "Server Closed";
                    StatusLightColor(1);
                }
            }
        }

        bool ServerOutputWindow_AutoScroll = true;
        //Handle data recieved from server
        private void ServerOutput_OutputDataRecieved(object sender, DataReceivedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (e.Data.Contains("WARN"))
                    {
                        ServerOutputWindow.AppendText(e.Data + "\n");
                        TextRange WarnOutputTextRange = new TextRange(ServerOutputWindow.Document.ContentEnd, ServerOutputWindow.Document.ContentEnd);
                        WarnOutputTextRange.Text = e.Data;
                        WarnOutputTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(WarningOutputColor));
                    }
                    else if (e.Data.Contains("ERROR"))
                    {
                        ServerOutputWindow.AppendText(e.Data + "\n");
                        TextRange ErrorOutputTextRange = new TextRange(ServerOutputWindow.Document.ContentEnd, ServerOutputWindow.Document.ContentEnd);
                        ErrorOutputTextRange.Text = e.Data;
                        ErrorOutputTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(ErrorOutputColor));
                    }
                    else if (e.Data.Contains("logged in with") || e.Data.Contains("left the game"))
                    {
                        ServerOutputWindow.AppendText(e.Data + "\n");
                        TextRange PlayEventTextRange = new TextRange(ServerOutputWindow.Document.ContentEnd, ServerOutputWindow.Document.ContentEnd);
                        PlayEventTextRange.Text = e.Data;
                        PlayEventTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(PlayerEventOutputColor));
                    }
                    else if (e.Data.Contains("Done") && e.Data.Contains("For help, type \"help\""))
                    {
                        ServerOutputWindow.AppendText(e.Data + "\n");
                        TextRange PlayEventTextRange = new TextRange(ServerOutputWindow.Document.ContentEnd, ServerOutputWindow.Document.ContentEnd);
                        PlayEventTextRange.Text = e.Data;
                        PlayEventTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(ServerDoneLoadingColor));
                    }
                    else
                    {
                        ServerOutputWindow.AppendText(e.Data + "\n");
                        TextRange DefaultOutputTextRange = new TextRange(ServerOutputWindow.Document.ContentEnd, ServerOutputWindow.Document.ContentEnd);
                        DefaultOutputTextRange.Text = e.Data;
                        DefaultOutputTextRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(DefaultOutputColor));
                    }
                }
                catch (Exception)
                {

                }

                if (ServerOutputWindow_AutoScroll) { ServerOutputWindow.ScrollToEnd(); }
            });
        }

        //Stop Server Handler
        private void ServerClose_Exited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (ServerProcess.HasExited && !StartStopServer.IsPressed)
                {
                    ServerIsRunning = false;
                    UpdateCpuRamUsageTimer(false);
                    ServerProcess.CancelOutputRead();
                    ServerProcess.OutputDataReceived -= new DataReceivedEventHandler(ServerOutput_OutputDataRecieved);
                    ServerProcess.Exited -= new EventHandler(ServerClose_Exited);
                    
                    StatusIndicator.Content = "Server Error";
                    StatusLightColor(0);

                    OnServerStop(false);
                }
            }));
        }

        //Commands
        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            string[] CommandsToSend = CommandBox.Text.Split(Convert.ToChar(";"));
            foreach (var item in CommandsToSend)
            {
                ServerProcess.StandardInput.WriteLine(item.Trim());
                CommandHistoryListBox.Items.Add(item.Trim());
            }
            CommandBox.Clear();
            //UpdateCommandHistory();
        }

        private void CommandBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                CommandBox.Clear();
            }
        }


        int CommandHistoryOffset = 0;


        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Show command hints
            if (e.Key == Key.Tab)
            {
                //ServerProcess.StandardInput.Write(CommandBox.Text + Key.Tab);
            }

            //Send Command(s)
            if (e.Key == Key.Enter && ServerIsRunning)
            {
                string[] CommandsToSend = CommandBox.Text.Split(';');
                foreach (var item in CommandsToSend)
                {
                    ServerProcess.StandardInput.WriteLine(item.Trim());
                    CommandHistoryListBox.Items.Add(item.Trim());
                }
                CommandBox.Clear();
            }
            /*
            if (e.Key == Key.Up) //Cycle through command history backwards
            {
                CommandBox.Text = CommandHistoryListBox.Items.GetItemAt(CommandHistoryListBox.Items.Count - 1 - CommandHistoryOffset).ToString();
            }

            if (e.Key == Key.Down) //Cycle through command history forwards
            { 

            }*/
        }

        private void CommandBox_LostFocus(object sender, RoutedEventArgs e) => CommandHistoryOffset = 0;

       /* private void QuickCommand(object sender, RoutedEventArgs e)
        {
            if (sender == KickAll)
            {
                if (File.Exists(@"c:\\temp\\servers\\1.12.2\\server.jar"))
                {
                    string JarPath = "c:\\temp\\1.12.2\\server.jar";
                    System.Windows.Forms.MessageBox.Show("Server already exists at  " + JarPath);
                }
                else
                {
                    Directory.CreateDirectory("c:\\temp\\servers\\1.12.2\\");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("https://download1475.mediafire.com/xh3vmjgsrlyg/ltrn90es9phkt6f/1.12.2.zip", @"c:\temp\1.12.2.zip");
                    ZipFile.ExtractToDirectory("c:\\temp\\1.12.2.zip", "c:\\temp\\servers\\1.12.2\\");
                    string JarPath = "c:\\temp\\1.12.2\\server.jar";
                    StartStopServerSettings.IsEnabled = true;
                    StartStopServer.IsEnabled = true;
                    System.Windows.Forms.MessageBox.Show("Your jar file is at " + JarPath);
                }

            }
            if (sender == BanAll)
            {
                if (File.Exists(@"c:\\temp\\servers\\1.8.9\\server.jar"))
                {
                    string JarPath = "c:\\temp\\1.8.9\\server.jar";
                    System.Windows.Forms.MessageBox.Show("Server already exists at  " + JarPath);
                }
                else
                {
                    Directory.CreateDirectory("c:\\temp\\servers\\1.8.9\\");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("https://download948.mediafire.com/y2gpdaw577hg/fcycblmefkw35vk/1.8.9.zip", @"c:\temp\1.8.9.zip");
                    ZipFile.ExtractToDirectory("c:\\temp\\1.8.9.zip", "c:\\temp\\servers\\1.8.9\\");
                    string JarPath = "c:\\temp\\1.8.9\\server.jar";
                    StartStopServerSettings.IsEnabled = true;
                    StartStopServer.IsEnabled = true;
                    System.Windows.Forms.MessageBox.Show("Your jar file is at " + JarPath);
                }
            }
            if (sender == opAll)
            {
                if (File.Exists(@"c:\\temp\\servers\\1.12.2forge\\forge-1.12.2-14.23.5.2859.jar"))
                {
                    string JarPath = "c:\\temp\\1.12.2forge\\forge-1.12.2-14.23.5.2859.jar";
                    System.Windows.Forms.MessageBox.Show("Server already exists at  " + JarPath);
                }
                else
                {
                    Directory.CreateDirectory("c:\\temp\\servers\\1.12.2forge\\");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("https://download1079.mediafire.com/vb739mi2tftg/ff3yqrp92tkox2d/forge1.12.2.zip", @"c:\temp\forge.zip");
                    ZipFile.ExtractToDirectory("c:\\temp\\forge.zip", "c:\\temp\\servers\\1.12.2forge\\");
                    string JarPath = "c:\\temp\\1.12.2forge\\forge-1.12.2-14.23.5.2859.jar";
                    StartStopServerSettings.IsEnabled = true;
                    StartStopServer.IsEnabled = true;
                    System.Windows.Forms.MessageBox.Show("Your jar file is at " + JarPath);
                }
            }
            if (sender == deopAll)
            {
                if (File.Exists(@"c:\\temp\\servers\\1.18\\server.jar"))
                {
                    string JarPath = "c:\\temp\\1.18\\server.jar";
                    System.Windows.Forms.MessageBox.Show("Server already exists at  " + JarPath);
                }
                else
                {
                    //ServerProcess.StandardInput.WriteLine("deop @a");
                    Directory.CreateDirectory("c:\\temp\\servers\\1.18\\");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("https://download853.mediafire.com/k0zfcvhbdagg/d3ylw1pp53xe7qh/1.18.zip", @"c:\temp\1.18.zip");
                    ZipFile.ExtractToDirectory("c:\\temp\\1.18.zip", "c:\\temp\\servers\\1.18\\");
                    string JarPath = "c:\\temp\\1.18\\server.jar";
                    StartStopServerSettings.IsEnabled = true;
                    StartStopServer.IsEnabled = true;
                    System.Windows.Forms.MessageBox.Show("Your jar file is at " + JarPath);
                }
            }
        }*/



        private void ServeroutputWindowScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ServeroutputWindowScale.Text != "" && ServeroutputWindowScale.Text != "%")
            {
                double scale = Convert.ToDouble(Regex.Replace(ServeroutputWindowScale.Text, "[^0-9]", "")); //Remove any non numeric characters from scale to use in code
                if (scale >= 1)
                {
                    ServerOutputWindow.FontSize = (scale / 100) * 12;
                }
            }
        }

        private void ServeroutputWindowScale_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ServeroutputWindowScale.Text[ServeroutputWindowScale.Text.Length - 1] != '%')
            {
                ServeroutputWindowScale.Text += "%";
            }
        }

        private void ServerOutputWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                ServerOutputWindow_AutoScroll = false;
                ServerOutputWindow_AutoScrollButton.Opacity = 1;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {/*
                //100% scale = font size 12
                //double scale = Convert.ToDouble(Regex.Replace(ServeroutputWindowScale.Text, "[^0-9]", ""));
                if (e.Delta > 0) //Scale Up
                {
                    ServerOutputWindow.FontSize += 1;
                    ServeroutputWindowScale.Text = (ServerOutputWindow.FontSize / 12) * 100 + "%";
                }
                if (e.Delta < 0) //Scale Down
                {
                    if (ServerOutputWindow.FontSize - 1 < 1)
                    {
                        ServerOutputWindow.FontSize = 1;
                    }
                    else
                    {
                        ServerOutputWindow.FontSize -= 1;
                    }
                    ServeroutputWindowScale.Text = (ServerOutputWindow.FontSize / 12) * 100 + "%";
                }*/
            }
        }

        private void ServerOutputWindow_AutoScrollButton_Click(object sender, RoutedEventArgs e)
        {
            ServerOutputWindow_AutoScroll = true;
            ServerOutputWindow_AutoScrollButton.Opacity = 0;
            ServerOutputWindow.ScrollToEnd();
        }

        private void CommandHistory_Execute_Click(object sender, RoutedEventArgs e)
        {
            ServerProcess.StandardInput.WriteLine(CommandHistoryListBox.SelectedItem.ToString());
        }

        private void CommandHistory_CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CommandHistoryListBox.SelectedItem.ToString());
        }

        private void CommandHistory_CopyToCommandBox_Click(object sender, RoutedEventArgs e)
        {
            CommandBox.Text = CommandHistoryListBox.SelectedItem.ToString();
        }

        private void CommandHistory_RemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            CommandHistoryListBox.Items.Remove(CommandHistoryListBox.SelectedItem);
        }

        //Notes Panel
        private void Notes_Import_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog BrowseTextFile = new WinForms.OpenFileDialog();
            BrowseTextFile.Filter = ".txt|*.txt";

            if (BrowseTextFile.ShowDialog() == WinForms.DialogResult.OK)
            {
                NotesRichTextBox.Document.Blocks.Clear();
                NotesRichTextBox.AppendText(File.ReadAllText(BrowseTextFile.FileName));
            }
        }

        private void Notes_AppendFromTxt_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog BrowseTextFile = new WinForms.OpenFileDialog();
            BrowseTextFile.Filter = ".txt|*.txt";

            if (BrowseTextFile.ShowDialog() == WinForms.DialogResult.OK)
            {
                NotesRichTextBox.AppendText(File.ReadAllText(BrowseTextFile.FileName));
            }
        }   

        private void Notes_Export_Click(object sender, RoutedEventArgs e)
        {/*
            WinForms.SaveFileDialog ExportNotes = new WinForms.SaveFileDialog();
            ExportNotes.Filter = "Text File (.txt)|*.txt|All Files|*.*";
            ExportNotes.Title = "Export notes to text file";

            if (ExportNotes.ShowDialog() == WinForms.DialogResult.OK && ExportNotes.FileName != null)
            {
                File.Create(ExportNotes.FileName);
                File.WriteAllText(ExportNotes.FileName, NotesTextBlock.Text);
            }*/
        }

        private void Notes_ExportToTxt_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog BrowseTextFile = new WinForms.OpenFileDialog();
            BrowseTextFile.Filter = ".txt|*.txt";

            if (BrowseTextFile.ShowDialog() == WinForms.DialogResult.OK)
            {
                File.AppendText(BrowseTextFile.FileName);
            }
        }

        private void NotesScale_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NotesScale.Text != "" && NotesScale.Text != "%")
            {
                double scale = Convert.ToDouble(Regex.Replace(NotesScale.Text, "[^0-9]", "")); //Remove any non numeric characters from scale to use in code
                if (scale >= 1)
                {
                    NotesRichTextBox.FontSize = (scale / 100) * 24;
                }
            }
        }

        private void NotesScale_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NotesScale.Text[NotesScale.Text.Length - 1] != '%')
            {
                NotesScale.Text += "%";
            }
        }

        private void NotesRichTextBox_TextChanged(object sender, TextChangedEventArgs e) => serverWrapper.Notes = new TextRange(NotesRichTextBox.Document.ContentStart, NotesRichTextBox.Document.ContentEnd).Text;
    }
}

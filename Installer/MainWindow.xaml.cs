using Ionic.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Threading;
using System.Reflection;
using IWshRuntimeLibrary;

namespace Installer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        static string appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SoundCloud Desktop";
        static string appExe = appDir + @"\SoundCloud Desktop.exe";
        static string version = "stable";

        public MainWindow() {
            var args = Environment.GetCommandLineArgs().ToList();

            // TODO: Choose version
            int i;
            if((i = args.IndexOf("-version")) != -1)
                version = args[i + 1];

            // Check if SCDesktop is running
            foreach(var proc in Process.GetProcesses()) {
                if(proc.ProcessName == "SoundCloud Desktop") {
                    var res = MessageBox.Show("The SoundCloud Desktop application is running. Do you want to close it to continue setup?", "Application running", MessageBoxButton.YesNo);
                    // Forces to close SCDesktop app
                    if(res == MessageBoxResult.Yes) {
                        proc.Kill();
                        proc.WaitForExit();
                    }
                    // Can't install while running
                    else
                        Environment.Exit(0);
                }
            }

            // Silent update
            if(args.Contains("-update")) {
                Install();
                Installed += (sender, e) => Exit();
            }

            InitializeComponent();

            // Install Dialog Events
            #region Install Dialog
            btnStart.Click += (sender, e) => {
                splashGrid.Visibility = Visibility.Hidden;
                installGrid.Visibility = Visibility.Visible;
                btnInstall.IsDefault = !(btnStart.IsDefault = false);
            };
            btnInstall.Click += (sender, e) => {
                var text = (string)btnInstall.Content;
                if(text == "Install") {
                    btnInstall.IsEnabled = false;
                    Install();
                }
                else if(text == "Finish") {
                    if(cbxDeskIco.IsChecked.HasValue && cbxDeskIco.IsChecked.Value)
                        CreateShortcut("SoundCloud Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), appExe);
                    Exit();
                }
            };
            Installed += (sender, e) => {
                btnInstall.Content = "Finish";
                btnInstall.IsEnabled = true;
                MessageBox.Show("Installation completed successfully!", "Installation successfull", MessageBoxButton.OK);
            };
            #endregion
        }

        #region Methods
        // Called when Installer has finished and is ready to exit
        event EventHandler Installed;
        void OnInstalled() {
            if(Installed != null)
                Installed(null, EventArgs.Empty);
        }

        // Downloads & installs the application
        void Install() {
            // Create folder for installing stuff
            if(!Directory.Exists(appDir))
                Directory.CreateDirectory(appDir);
            // Clear all files to clean Application Folder (except folders & .json files)
            else {
                var files = new DirectoryInfo(appDir).GetFiles();
                foreach(var file in files) {
                    if(file.Extension != (".json") || file.FullName != "installer.exe")
                        file.Delete();
                }
            }

            // Create a webclient for downloading
            var wc = new WebClient();
            wc.DownloadDataCompleted += (sender, e) => {
                var thread = new Thread(new ThreadStart(() => {
                    // If error occurred then throw error
                    if(e.Cancelled || e.Error != null)
                        Error(e.Error != null ? e.Error.Message : "The download of the files was cancelled!");

                    // Copy to directory
                    using(var stream = new MemoryStream(e.Result)) {
                        using(var zip = ZipFile.Read(stream)) {
                            zip.ZipErrorAction = ZipErrorAction.Throw;
                            zip.ExtractAll(appDir, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                    Dispatcher.Invoke(new Action(OnInstalled));
                }));
                thread.Start();
            };
            wc.DownloadProgressChanged += (sender, e) => pbar.Value = e.ProgressPercentage;
            wc.DownloadDataAsync(new Uri("http://scdesktop.us.to/api/download.php?version=" + version));
        }

        // Exists the application & Starts SCDesktop
        void Exit() {
            Process.Start(appDir + "\\SoundCloud Desktop.exe");
            // Remove itself when done
            var Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + Assembly.GetExecutingAssembly().Location;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            // Exit Application
            Environment.Exit(0);
        }
        // Displays an error message and closes the application
        void Error(string msg) {
            MessageBox.Show(msg, "Fatal Error", MessageBoxButton.OK);
            Environment.Exit(0);
        }

        // Creates a new shortcut
        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation) {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            // Properties
            shortcut.Description = "The #1 Free SoundCloud application on your desktop";  // The description of the shortcut
            shortcut.IconLocation = appExe + ", 0";            // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;          // The path of the file that will launch when the shortcut is run
            shortcut.Save();
        }
        #endregion
    }
}

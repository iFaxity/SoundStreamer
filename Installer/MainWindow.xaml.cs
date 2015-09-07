using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            var args = Environment.GetCommandLineArgs().ToList();
            InitializeComponent();

            foreach(var proc in Process.GetProcesses()) {
                if(proc.ProcessName == "SoundCloud.Desktop") {
                    var res = MessageBox.Show("Please close the 'SoundCloud Desktop' application to continue. Press 'Yes' after you have closed it.", "Application not closed", MessageBoxButton.OKCancel);

                    if(res == MessageBoxResult.Cancel)
                        Environment.Exit(0);
                }
            }

            // Only update
            if(args.Contains("update")) {
                if(File.Exists("SoundCloud.Desktop.exe")) {
                    while(Core.IsFileOpen("SoundCloud.Desktop.exe")) {
                        MessageBox.Show("Please close the 'SoundCloud Desktop' to continue updating.", "Update prevented", MessageBoxButton.OK);
                    }
                    Core.Install("");
                }
                Environment.Exit(0);
            }
            // Force install
            else if(args.Contains("force")) {

            }
            // Install dialog
            else {

            }
        }
    }
}

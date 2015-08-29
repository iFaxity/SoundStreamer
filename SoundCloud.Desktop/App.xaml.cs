using System;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace SoundCloud {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            string baseDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", baseDirectory + @"\settings.config");
        }
    }
}

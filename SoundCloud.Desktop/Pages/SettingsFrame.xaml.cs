using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Me.xaml
    /// </summary>
    public partial class SettingsFrame : Page {
        public SettingsFrame() {
            InitializeComponent();

            cmbAudioDevice.SelectionChanged += (sender, e) => {
                Player.SetAudioDevice(cmbAudioDevice.SelectedIndex);
                AppSettings.Settings["device"] = Player.AvailableDevices[cmbAudioDevice.SelectedIndex].id;
            };

            // Load device from settings
            string device = "";
            if(AppSettings.Settings.ContainsKey("device"))
                device = (string)AppSettings.Settings["device"];

            // Add no sound selection
            cmbAudioDevice.Items.Add("<No Sound>");
            var list = Player.AvailableDevices;
            for(int i = 1; i < list.Count; i++) {
                cmbAudioDevice.Items.Add(list[i].name.Contains("(") ? list[i].name.Substring(0, list[i].name.IndexOf("(")) : list[i].name);
                if(list[i].id == device)
                    cmbAudioDevice.SelectedIndex = i;
            }

            // Clear login
            btnClearLogin.Click += (sender, e) => System.IO.File.Delete("login.json");
            cbxAutoUpdate.Checked += cbxAutoUpdate_Changed;
            cbxAutoUpdate.Unchecked += cbxAutoUpdate_Changed;
        }

        void cbxAutoUpdate_Changed(object sender, RoutedEventArgs e) {
            if(!cbxAutoUpdate.IsChecked.HasValue)
                return;
            if(AppSettings.Settings.ContainsKey("autoUpdate"))
                AppSettings.Settings["autoupdate"] = cbxAutoUpdate.IsChecked;
            else
                AppSettings.Settings.Add("autoUpdate", cbxAutoUpdate.IsChecked);
        }
        void btnCheckUpdate_Click(object sender, RoutedEventArgs e) { AppSettings.CheckUpdate(); }

        void Link_MouseEnter(object sender, MouseEventArgs e) {
            var o = (sender as FaxUi.IcoMoon);
            if(o.Icon == FaxUi.MoonIcon.SoundCloud)
                o.Foreground = UICore.FromHex("#FFFF8531");
            else if(o.Icon == FaxUi.MoonIcon.Globe)
                o.Foreground = UICore.FromHex("#FF3FA8FF");
        }
        void Link_MouseLeave(object sender, MouseEventArgs e) {
            var o = (sender as FaxUi.IcoMoon);
            if(o.Icon == FaxUi.MoonIcon.SoundCloud)
                o.Foreground = UICore.FromHex("#FFFF6800");
            else if(o.Icon == FaxUi.MoonIcon.Globe)
                o.Foreground = UICore.FromHex("#FF008BFF");
        }
        void Link_MouseDown(object sender, MouseButtonEventArgs e) {
            var icon = (sender as FaxUi.IcoMoon).Icon;
            var url = "";
            if(icon == FaxUi.MoonIcon.SoundCloud)
                url = "https://soundcloud.com/faxity";
            else if(icon == FaxUi.MoonIcon.Globe)
                url = "http://scdesktop.us.to";
            else
                return;

            System.Diagnostics.Process.Start(url);
        }
    }
}

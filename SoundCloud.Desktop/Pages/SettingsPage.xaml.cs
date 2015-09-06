using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Me.xaml
    /// </summary>
    public partial class SettingsPage : Page {
        public SettingsPage() {
            InitializeComponent();

            // Add devices to list and set device to the saved one
            var list = Player.AvailableDevices;
            for(int i = 1; i < list.Count; i++) {
                cmbAudioDevice.Items.Add(list[i].name.Contains("(") ? list[i].name.Substring(0, list[i].name.IndexOf("(")) : list[i].name);
                if(list[i].id == AppSettings.Settings.GetValue<string>("Device"))
                    cmbAudioDevice.SelectedIndex = i;
            }

            // Audio Device Combo Box
            cmbAudioDevice.SelectionChanged += cmbAudioDevice_SelectionChanged;
            // Clear login
            btnClearLogin.Click += (sender, e) => {
                AppSettings.Settings.Reset("Username");
                AppSettings.Settings.Reset("Password");
            };

            // Hotkeys Toggle
            cbxAutoUpdate.IsChecked = AppSettings.Settings.GetValue<bool>("AutoUpdate");
            cbxAutoUpdate.Checked += cbxAutoUpdate_Changed;
            cbxAutoUpdate.Unchecked += cbxAutoUpdate_Changed;

            // Hotkeys Toggle
            cbxHotkeys.IsChecked = AppSettings.Settings.GetValue<bool>("HotkeysEnabled");
            cbxHotkeys.Checked += cbxHotkeys_Changed;
            cbxHotkeys.Unchecked += cbxHotkeys_Changed;
        }

        void cmbAudioDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Player.SetAudioDevice(cmbAudioDevice.SelectedIndex);
            var id = Player.AvailableDevices[cmbAudioDevice.SelectedIndex].id;
            if(id != null)
                AppSettings.Settings.SetValue("Device", id);
        }

        void cbxAutoUpdate_Changed(object sender, RoutedEventArgs e) {
            if(cbxAutoUpdate.IsChecked.HasValue)
                AppSettings.Settings.SetValue("AutoUpdate", cbxAutoUpdate.IsChecked.Value);
        }
        void cbxHotkeys_Changed(object sender, RoutedEventArgs e) {
            if(cbxHotkeys.IsChecked.HasValue)
                AppSettings.Settings.SetValue("HotkeysEnabled", cbxHotkeys.IsChecked.Value);
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

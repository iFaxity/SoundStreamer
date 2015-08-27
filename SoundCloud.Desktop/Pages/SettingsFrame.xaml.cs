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
                AppSettings.Settings["device"] = Player.AvailableDevices[cmbAudioDevice.SelectedIndex].driver;
            };

            var list = Player.AvailableDevices;

            // Add no sound selection
            cmbAudioDevice.Items.Add("<No Sound>");
            for(int i = 1; i < list.Count; i++) {
                var name = list[i].name;
                if(name.Contains('(')) name = list[i].name.Substring(0, list[i].name.IndexOf('(') - 1);

                cmbAudioDevice.Items.Add(name);
                if(list[i].IsDefault) cmbAudioDevice.SelectedIndex = i;
            }

            if(!AppSettings.Settings.ContainsKey("device"))
                AppSettings.Settings.Add("device", list[1].driver);

            for(int i = 0; i < list.Count; i++)
                if(list[i].driver == (string)AppSettings.Settings["device"]) {
                    cmbAudioDevice.SelectedIndex = i;
                    Player.SetAudioDevice(i);
                }


            // Clear login
            btnClearLogin.Click += (sender, e) => System.IO.File.Delete("login.json");
        }
    }
}

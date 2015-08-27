using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaxLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

using System.Runtime.Serialization.Json;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Windows.Media;

using Streamer.Net.SoundCloud;
using Un4seen.Bass;

using FaxLib.Input.WPF;
using FaxLib.Input;
using System.Net;
using System.Diagnostics;

namespace SoundCloud.Desktop {
    public static class Shortcuts {
        // HotKeyHandler
        static HotKeyHandler handler;
        // Actions
        public static Action<object> actionPlay;
        public static Action<object> actionNext;
        public static Action<object> actionPrev;
        public static Action<object> actionVolUp;
        public static Action<object> actionVolDown;

        public static void Init(MainWindow window) {
            actionPlay = new Action<object>((o) => window.playerPlay_MouseDown(window, null));
            actionNext = new Action<object>((o) => window.playerNext_MouseDown(window, null));
            actionPrev = new Action<object>((o) => window.playerPrev_MouseDown(window, null));

            actionVolUp = new Action<object>((o) => window.ChangeVolume(Player.Volume + (float)o));
            actionVolDown = new Action<object>((o) => window.ChangeVolume(Player.Volume - (float)o));

            handler = new HotKeyHandler(window);
            Update();
        }

        public static void Update() {
            handler.UnregisterAll();

            handler.RegisterKey(new HotKey(Key.Up, ModifierKey.Shift, actionPlay, null));
            handler.RegisterKey(new HotKey(Key.Right, ModifierKey.Shift, actionNext, null));
            handler.RegisterKey(new HotKey(Key.Left, ModifierKey.Shift, actionPrev, null));
            handler.RegisterKey(new HotKey(Key.PageUp, ModifierKey.Shift, actionVolUp, 0.05f));
            handler.RegisterKey(new HotKey(Key.PageDown, ModifierKey.Shift, actionVolDown, 0.05f));
        }
    }

    public static class UICore {
        /// <summary>
        /// Toggles spinner on the grid. If it toggled on then it returns true if not it returns false.
        /// </summary>
        public static bool ToggleSpinner(Grid grid, double size = 100) {
            return ToggleSpinner(grid, size, Colors.Gray);
        }
        /// <summary>
        /// Toggles spinner on the grid. If it toggled on then it returns true if not it returns false.
        /// </summary>
        public static bool ToggleSpinner(Grid grid, double size, Color color) {
            bool state = false;
            grid.Dispatcher.Invoke(() => {
                var spinner = FindChild<FaxUi.IcoMoon>(grid, "spinner");
                if(spinner != null)
                    grid.Children.Remove(spinner);
                else {
                    var spin = new FaxUi.IcoMoon {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,

                        Icon = FaxUi.MoonIcon.Spinner,
                        Width = size,
                        Height = size,
                        Foreground = new SolidColorBrush(color),
                        Spin = true,
                        SpinDuration = 1,
                        Name = "spinner",
                    };

                    spin.Name = "spinner";

                    grid.Children.Add(spin);
                    state = true;
                }
            });
            return state;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject {
            // Confirm parent and childName are valid. 
            if(parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if(childType == null) {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if(foundChild != null) break;
                }
                else if(!string.IsNullOrEmpty(childName)) {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if(frameworkElement != null && frameworkElement.Name == childName) {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }

    public class AppSettings {
        const string settingJsonPath = "settings.json";

        public static Dictionary<string, object> Settings { get; set; }
        public static List<Playlist> Playlists { get; set; }

        // Saves settings
        public static void Save() {
            var json = new Dictionary<string, object>();
            json.Add("settings", Settings);
            json.Add("playlists", Playlists);
            File.WriteAllText(settingJsonPath, JsonConvert.SerializeObject(json, Formatting.Indented));
        }
        // Loads settings
        public static void Load() {
            // Check for file if not initialize everything
            if(!File.Exists(settingJsonPath)) {
                Settings = new Dictionary<string, object>();
                Playlists = new List<Playlist>();
                return;
            }

            // Load json file for all information
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(settingJsonPath));
            Settings = json["settings"] as Dictionary<string, object>;
            if(Settings == null)
                Settings = new Dictionary<string, object>();

            Playlists = json["playlists"] as List<Playlist>;
            if(Playlists == null)
                Playlists = new List<Playlist>();
        }

        const string version = "0.1 Stable";
        public static bool IsUpdated() {
            var latest = new WebClient().DownloadString(new Uri("http://scdesktop.us.to/version/latest?v=stable"));
            var current = int.Parse(version.Substring(0, version.IndexOf(" ") - 1).Replace(".", ""));
            return current >= int.Parse(latest.Replace(".", ""));
        }

        public static void Update() {
            var wc = new WebClient();
            wc.DownloadFileCompleted += (o, args) => {
                if(!File.Exists("install.exe"))
                    throw new Exception("Unable to update. Intaller not found!");
                Process.Start("install.exe", "-hidden");
                Environment.Exit(0);
            };
            wc.DownloadFileAsync(new Uri("http://scdesktop.us.to/api/latest.php?v=stable"), "install.exe");
        }
    }
}

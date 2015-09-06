using FaxLib;
using FaxLib.Input.WPF;
using Newtonsoft.Json;
using Streamer.Net.SoundCloud;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    // Static UI specific functions
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

        public static Brush FromHex(string hex) {
            if(hex.Length == 9 && hex.StartsWith("#"))
                return new BrushConverter().ConvertFrom(hex) as Brush;
            else
                throw new Exception("Brush hex was invalid.");
        }
    }

    // Handles Updates & Settings
    public class AppSettings {
        const string playlistsPath = "playlists.json";
        const string settingsPath = "settings.json";

        public static Settings Settings { get; set; }
        // Version of the application
        const string version = "1.0.0";
        public static List<Playlist> Playlists { get; set; }

        // Saves settings
        public static void Save() {
            File.WriteAllText(playlistsPath, JsonConvert.SerializeObject(Playlists, Formatting.Indented));
            Settings.Save();
        }
        // Loads settings
        public static void Load() {
            // Event for auto saving settings
            Settings = new Settings(settingsPath, true);

            // Read default settings from resource
            if(!Settings.Load(settingsPath)) {
                using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SoundCloud.Desktop.settings.json"))
                using(StreamReader reader = new StreamReader(stream)) {
                    Settings.Load(reader.ReadToEnd());
                }
            }

            // Check for file if not initialize everything
            if(!File.Exists(playlistsPath))
                Playlists = new List<Playlist>();
            // Load Playlists
            else
                Playlists = JsonConvert.DeserializeObject<List<Playlist>>(File.ReadAllText(playlistsPath)) ?? new List<Playlist>();

            // Set audio device from settings
            var device = Settings.GetValue<string>("Device");
            var devices = Player.AvailableDevices;
            for(int i = 1; i < devices.Count; i++) {
                if(devices[i].id == device)
                    Player.SetAudioDevice(i);
            }
        }

        // Checks for updates and prompts the user to update
        public static void CheckUpdate() {
            try {
                var latest = new WebClient().DownloadString(new Uri("http://scdesktop.us.to/api/versions.php?v=stable"));
                var current = int.Parse(version.Replace(".", ""));
                if(current < int.Parse(latest.Replace(".", ""))) {
                    var res = MessageBox.Show("A new update was found. Do you want to update now? This will close the application.", "New update found", MessageBoxButton.YesNo);
                    if(res == MessageBoxResult.Yes)
                        Update(latest);
                }
            }
            catch(Exception ex) {
                MessageBox.Show("Error checking for updates. Error: " + ex.Message, "error checking for updates", MessageBoxButton.OK);
            }
        }

        // Force Update the application
        public static void Update(string version) {
            var wc = new WebClient();
            wc.DownloadFileCompleted += (o, args) => {
                if(!File.Exists("install.exe"))
                    throw new Exception("Unable to update. Intaller not found!");
                // Self extracting archive 7Zip SFX
                Process.Start("install.exe", "-y -gm2 -sd1");
                Environment.Exit(0);
            };
            wc.DownloadFileAsync(new Uri("http://scdesktop.us.to/api/download.php?v=" + version), "install.exe");
        }
    }

    // Extension Methods
    public static class Extensions {
        /// <summary>
        /// Formats into the closest format
        /// </summary>
        /// <param name="time">TimeSpan to format</param>
        public static string ToTime(this TimeSpan time) {
            return time.ToString(time.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");
        }
    }
}

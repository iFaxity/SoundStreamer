﻿// SoundStreamer Todo List:
//TODO: Following Tab         - People you follow & people following you
//TODO: User pages            - Click user to get user's tracks and other info.
//TODO: Customisable Hotkeys  - Custom hotkey combinations
//TODO: Offline Mode          - Play downloaded tracks & Playlists
//TODO: Create Playlists      - Create Online and Offline Playlists.
//TODO: User Features         - Like tracks & Playlists, Follow people.
//TODO: More Settings         - Audio visualizer toggler & Custom Hotkeys

using Streamer.SoundCloud;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Un4seen.Bass;

namespace SoundStreamer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FaxUI.UIWindow {
        #region Properties
        //Application ID
        internal const string ClientID = "589e0ff400a0bc83ecd8eb20b94b57de";
        internal const string ClientSecret = "4194ded960527644e96fea6cedab671d";

        internal LikePage likePage;
        internal SettingsPage settingsPage;
        internal StreamPage streamPage;

        DispatcherTimer timer = new DispatcherTimer();
        string menuSelected = "";
        public static MainWindow window;
        #endregion

        // Login and Initialize Application UI
        public MainWindow() {
            window = this;
            // Loads all the settings
            Player.Init(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            AppSettings.Load();
            // Update check
            if(AppSettings.Settings.GetValue<bool>("AutoUpdate"))
                AppSettings.CheckUpdate(false);

            InitializeComponent();

            #region Events
            // Safely frees all the bass resources and saves all settings
            Closing += OnClosing;
            spectrumSize.MouseDown += spectrumSize_MouseDown;
            spectrumClose.MouseDown += spectrumClose_MouseDown;

            // Player Events
            Player.SongEnded += (sender, e) => Player.Play(Player.TrackIndex + 1);
            Player.SongStarted += Player_SongStarted;

            // Player Progressbar & Progressbar Popup Events
            playerBbar.MouseDown += playerPop_MouseDown;
            playerBbar.MouseLeave += playerBbar_MouseLeave;
            playerBbar.MouseEnter += (sender, e) => playerPop.IsOpen = true;
            playerBbar.MouseMove += playerBbar_MouseMove;

            // Volume Popup Events
            playerVolPop.MouseLeave += playerVolPop_MouseLeave;
            playerVolSlider.ValueChanged += playerVolSlider_ValueChanged;
            playerVol.MouseDown += playerVol_MouseDown;
            playerVol.MouseEnter += playerVol_MouseEnter;
            playerVol.MouseLeave += playerVol_MouseLeave;

            // Player buttons
            playerPlay.MouseDown += playerPlay_MouseDown;
            playerPrev.MouseDown += playerPrev_MouseDown;
            playerNext.MouseDown += playerNext_MouseDown;

            // Search Box Events
            tbxSearch.KeyDown += (sender, e) => {
                if(e.Key == Key.Return) {
                    contentFrame.Navigate(new SearchPage(tbxSearch.Text));
                    menuSelected = "search";
                }
            };
            tbxSearch.GotFocus += (sender, e) => tbxSearch.Text = "";
            tbxSearch.LostFocus += (sender, e) => {
                if(string.IsNullOrWhiteSpace(tbxSearch.Text))
                    tbxSearch.Text = "Search";
            };
            #endregion

            // Player Timer
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += OnTick;
            timer.Start();

            // Add event to every menu element
            foreach(TextBlock item in mainStack.Children) {
                item.MouseDown += Menu_Click;
                if(item.Name != "mainTitle") {
                    item.MouseLeave += Menu_Leave;
                    item.MouseEnter += Menu_Enter;
                }
            }

            // Init Shortcut Keys
            if(AppSettings.Settings.GetValue<bool>("HotkeysEnabled"))
                Shortcuts.Init(this);

            // Navigate to login page
            contentFrame.Navigate(new LoginPage());
            menuSelected = "stream";
        }

        #region Menu Events
        public void Menu_Click(object sender, MouseEventArgs e) {
            if(!SoundCloudCore.IsConnected)
                return;
            // Open page in frame
            switch(menuSelected = ((TextBlock)sender).Text.ToLower()) {
                case "stream":
                    contentFrame.Navigate(streamPage = streamPage ?? new StreamPage());
                    return;
                case "likes":
                    contentFrame.Navigate(likePage = likePage ?? new LikePage());
                    return;
                case "my tracks":
                    return;
                case "settings":
                    contentFrame.Navigate(settingsPage = settingsPage ?? new SettingsPage());
                    return;
            }
        }
        public void Menu_Enter(object sender, MouseEventArgs e) {
            Cursor = Cursors.Hand;
            var t = (TextBlock)sender;
            t.Padding = new Thickness(5, 0, 0, 0);
            t.Foreground = t.Foreground = Core.FromHex("#FFC8C8C8");
        }
        public void Menu_Leave(object sender, MouseEventArgs e) {
            Cursor = Cursors.Arrow;
            var t = (TextBlock)sender;
            t.Padding = new Thickness(0);
            t.Foreground = Core.FromHex("#FFB7B7B7");
        }
        #endregion

        #region Player Events
        public void Player_MouseEnter(object sender, MouseEventArgs e) {
            var ico = (FaxUI.IcoMoon)sender;
            ico.Foreground = Core.FromHex("#FF666666"); // FFF5F5F5
        }
        public void Player_MouseLeave(object sender, MouseEventArgs e) {
            var ico = (FaxUI.IcoMoon)sender;
            ico.Foreground = Core.FromHex("#FFD3D3D3");
        }
        public void Player_SongStarted(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                // Set track info and start the track
                var track = (Track)sender;
                playerTotal.Text = track.Duration.ToTime();
                playerTitle.Text = track.Title;
                playerPlay.Icon = FaxUI.MoonIcon.Pause;
                Player.Volume = (float)playerVolSlider.Value;

                // Get Track Image
                if(!string.IsNullOrEmpty(track.ArtworkUrl))
                    playerImg.Source = new BitmapImage(new Uri(track.ArtworkUrl, UriKind.Absolute));
            });
        }
        public void ChangeVolume(float volume) {
            playerVolSlider.Value = volume;
        }

        public void playerPlay_MouseDown(object sender, MouseButtonEventArgs e) {
            if(Player.State == BASSActive.BASS_ACTIVE_STOPPED)
                return;

            if(Player.State != BASSActive.BASS_ACTIVE_PLAYING) {
                Player.Resume();
                playerPlay.Icon = FaxUI.MoonIcon.Pause;
            }
            else {
                Player.Pause();
                playerPlay.Icon = FaxUI.MoonIcon.Play;
            }
        }
        public void playerPrev_MouseDown(object sender, MouseButtonEventArgs e) { Player.Play(Player.TrackIndex - 1); }
        public void playerNext_MouseDown(object sender, MouseButtonEventArgs e) { Player.Play(Player.TrackIndex + 1); }

        public void playerPop_MouseDown(object sender, MouseButtonEventArgs e) {
            if(Player.State != BASSActive.BASS_ACTIVE_STOPPED) {
                var pos = e.GetPosition(playerPbar).X;
                Player.SetPos(playerPbar.Value = (pos / playerPbar.ActualWidth) * playerPbar.MaxValue);
            }
        }
        public void playerBbar_MouseLeave(object sender, MouseEventArgs e) {
            if(!playerBbar.IsMouseOver)
                playerPop.IsOpen = false;
        }
        public void playerBbar_MouseMove(object sender, MouseEventArgs e) {
            var pos = e.GetPosition(playerPbar).X;
            playerPop.HorizontalOffset = pos - 14; //Offset is 14

            if(Player.State != BASSActive.BASS_ACTIVE_STOPPED) {
                var time = TimeSpan.FromSeconds((pos / playerPbar.ActualWidth) * SoundCloudCore.Tracks[Player.Tracks[Player.TrackIndex]].Duration.TotalSeconds);
                playerPopTxt.Text = time.ToTime();
            }
        }

        public void playerVolPop_MouseLeave(object sender, MouseEventArgs e) {
            if(!playerVol.IsMouseOver)
                playerVolPop.IsOpen = false;
        }
        public void playerVolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Player.Volume = (float)e.NewValue;
            if(e.NewValue > 0.75f)
                playerVol.Icon = FaxUI.MoonIcon.VolumeHigh;
            else if(e.NewValue > 0.5f)
                playerVol.Icon = FaxUI.MoonIcon.VolumeMedium;
            else if(e.NewValue > 0f)
                playerVol.Icon = FaxUI.MoonIcon.VolumeLow;
            else
                playerVol.Icon = FaxUI.MoonIcon.VolumeOff;
        }

        public void playerVol_MouseDown(object sender, MouseButtonEventArgs e) {
            if(playerVolSlider.Value > 0) {
                playerVolSlider.Value = 0;
                playerVol.Icon = FaxUI.MoonIcon.VolumeMute;
            }
            else {
                playerVolSlider.Value = 1;
                playerVol.Icon = FaxUI.MoonIcon.VolumeHigh;
            }
        }
        public void playerVol_MouseEnter(object sender, MouseEventArgs e) {
            playerVolPop.IsOpen = true;
            Player_MouseEnter(sender, e);
        }
        public void playerVol_MouseLeave(object sender, MouseEventArgs e) {
            if(!playerVolPop.IsMouseOver) {
                playerVolPop.IsOpen = false;
                Player_MouseLeave(sender, e);
            }
        }
        #endregion

        #region Spectrum & Misc Events
        public void OnTick(object sender, EventArgs e) {
            // Stop if nothing is happening
            if(Player.State == BASSActive.BASS_ACTIVE_STOPPED)
                return;
            playerBbar.Value = Player.GetBufferedPercent(); //Buffer Bar

            // Stop if nothing is playing
            if(Player.State != BASSActive.BASS_ACTIVE_PLAYING) return;

            // Update player bar progress
            var current = Player.GetPos();
            var total = Player.GetTotal();

            playerPbar.Value = (current.TotalSeconds / total.TotalSeconds) * playerPbar.MaxValue;
            playerCurrent.Text = current.ToTime(); // Set text of current timespan

            // Update player button
            if(Player.State != BASSActive.BASS_ACTIVE_PLAYING)
                playerPlay.Icon = FaxUI.MoonIcon.Play;
        }
        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            Hide();
            AppSettings.Save();
            Bass.BASS_Free();
        }
        public void spectrumSize_MouseDown(object sender, MouseButtonEventArgs e) {
            if(spectrum.BarCount > 32) {
                spectrum.BarCount = 32;
                spectrum.BarWidth = 10;
                spectrum.Height = 150;
                spectrumSize.Icon = FaxUI.MoonIcon.Expand2;
            }
            else {
                spectrum.BarCount = 64;
                spectrum.BarWidth = 20;
                spectrum.Height = 200;
                spectrumSize.Icon = FaxUI.MoonIcon.Collapse2;
            }
        }
        public void spectrumClose_MouseDown(object sender, MouseButtonEventArgs e) {
            if(spectrum.Visibility == Visibility.Visible) {
                spectrum.Visibility = Visibility.Collapsed;
                spectrumClose.Icon = FaxUI.MoonIcon.ArrowUp;
            }
            else {
                spectrum.Visibility = Visibility.Visible;
                spectrumClose.Icon = FaxUI.MoonIcon.ArrowDown;
            }
        }
        #endregion

        #region UI Features
        /// <summary>
        /// Creates a Brush from a hex value
        /// </summary>
        /// <param name="hex">ARGB Hex Color ex. #FF000000, #FFFFFFFF</param>
        /// <returns></returns>
        public static Brush FromHex(string hex) {
            if(hex.Length == 9 && hex.StartsWith("#"))
                return (Brush)new BrushConverter().ConvertFrom(hex);
            else
                throw new ArgumentException("Brush hex was invalid", "hex");
        }

        /// <summary>
        /// Toggles spinner on the grid. If it toggled on then it returns true if not it returns false.
        /// </summary>
        public bool ToggleSpinner(Grid grid, Color color, double size = 100) {
            grid.Dispatcher.Invoke(() => {
                var spinner = grid.FindChild<FaxUI.IcoMoon>("spinner");
                if(spinner != null) {
                    grid.Children.Remove(spinner);
                    return true;
                }
                else {
                    var spin = new FaxUI.IcoMoon {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,

                        Icon = FaxUI.MoonIcon.Spinner,
                        Width = size,
                        Height = size,
                        Foreground = new SolidColorBrush(color),
                        Spin = true,
                        SpinDuration = 1,
                        Name = "spinner",
                    };

                    spin.Name = "spinner";
                    grid.Children.Add(spin);
                    return true;
                }
            });
            return false;
        }
        #endregion
    }
}
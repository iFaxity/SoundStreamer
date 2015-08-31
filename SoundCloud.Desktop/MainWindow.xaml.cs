using Streamer.Net.SoundCloud;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Un4seen.Bass;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FaxUi.UiWindow {
        #region Properties
        //Application ID
        internal const string ClientID = @"589e0ff400a0bc83ecd8eb20b94b57de";
        internal const string ClientSecret = @"4194ded960527644e96fea6cedab671d";

        internal LikePage likePage;
        internal SettingsPage settingsPage;
        internal StreamPage streamPage;

        DispatcherTimer timer = new DispatcherTimer();
        string menuSelected = "";
        #endregion

        // Login and Initialize Application UI
        public MainWindow() {
            // Loads all the settings
            Player.Init(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            AppSettings.Load();
            // Update check
            if(Properties.Settings.Default.AutoUpdate)
                AppSettings.CheckUpdate();

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
            if(Properties.Settings.Default.HotkeysEnabled)
                Shortcuts.Init(this);

            // Navigate to login page
            contentFrame.Navigate(new LoginPage(this));
            menuSelected = "stream";
        }

        #region Menu Events
        public void Menu_Click(object sender, MouseEventArgs e) {
            var text = ((TextBlock)sender).Text.ToLower();
            if(!SoundCloudClient.IsConnected || menuSelected == text)
                return;

            //Indicate which menu item is selected
            menuSelected = text;
            switch(menuSelected) {
                case "stream":
                    contentFrame.Navigate((streamPage = streamPage ?? new StreamPage()));
                    return;
                case "likes":
                    contentFrame.Navigate((likePage = likePage ?? new LikePage()));
                    return;
                case "my tracks":
                    return;
                case "settings":
                    contentFrame.Navigate((settingsPage = settingsPage ?? new SettingsPage()));
                    return;
            }
        }
        public void Menu_Enter(object sender, MouseEventArgs e) {
            Cursor = Cursors.Hand;
            var t = sender as TextBlock;
            t.Padding = new Thickness(5, 0, 0, 0);
            t.Foreground = t.Foreground = UICore.FromHex("#FFC8C8C8");
        }
        public void Menu_Leave(object sender, MouseEventArgs e) {
            Cursor = Cursors.Arrow;
            var t = sender as TextBlock;
            t.Padding = new Thickness(0);
            t.Foreground = UICore.FromHex("#FFB7B7B7");
        }
        #endregion

        #region Player Events
        public void Player_MouseEnter(object sender, MouseEventArgs e) {
            var ico = sender as FaxUi.IcoMoon;
            ico.Foreground = UICore.FromHex("#FF666666"); // FFF5F5F5
        }
        public void Player_MouseLeave(object sender, MouseEventArgs e) {
            var ico = sender as FaxUi.IcoMoon;
            ico.Foreground = UICore.FromHex("#FFD3D3D3");
        }
        public void Player_SongStarted(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                // Set track info and start the track
                var track = (Track)sender;
                playerTotal.Text = track.Duration.ToTime();
                playerTitle.Text = track.Title;
                playerPlay.Icon = FaxUi.MoonIcon.Pause;
                Player.Volume = (float)playerVolSlider.Value;

                // Get Track Image
                if(!string.IsNullOrEmpty(track.ArtworkUrl))
                    playerImg.Source = new BitmapImage(new Uri(track.ArtworkUrl, UriKind.Absolute));
            });
        }
        public void ChangeVolume(float volume) { playerVolSlider.Value = volume; }

        public void playerPlay_MouseDown(object sender, MouseButtonEventArgs e) {
            if(Player.State == BASSActive.BASS_ACTIVE_STOPPED)
                return;

            if(Player.State != BASSActive.BASS_ACTIVE_PLAYING) {
                Player.Resume();
                playerPlay.Icon = FaxUi.MoonIcon.Pause;
            }
            else {
                Player.Pause();
                playerPlay.Icon = FaxUi.MoonIcon.Play;
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
                var time = TimeSpan.FromSeconds((pos / playerPbar.ActualWidth) * SoundCloudClient.Collection[Player.Tracks[Player.TrackIndex]].Duration.TotalSeconds);
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
                playerVol.Icon = FaxUi.MoonIcon.VolumeHigh;
            else if(e.NewValue > 0.5f)
                playerVol.Icon = FaxUi.MoonIcon.VolumeMedium;
            else if(e.NewValue > 0f)
                playerVol.Icon = FaxUi.MoonIcon.VolumeLow;
            else
                playerVol.Icon = FaxUi.MoonIcon.VolumeOff;
        }

        public void playerVol_MouseDown(object sender, MouseButtonEventArgs e) {
            if(playerVolSlider.Value > 0) {
                playerVolSlider.Value = 0;
                playerVol.Icon = FaxUi.MoonIcon.VolumeMute;
            }
            else {
                playerVolSlider.Value = 1;
                playerVol.Icon = FaxUi.MoonIcon.VolumeHigh;
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
                playerPlay.Icon = FaxUi.MoonIcon.Play;
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
                spectrumSize.Icon = FaxUi.MoonIcon.Expand2;
            }
            else {
                spectrum.BarCount = 64;
                spectrum.BarWidth = 20;
                spectrum.Height = 200;
                spectrumSize.Icon = FaxUi.MoonIcon.Collapse2;
            }
        }
        public void spectrumClose_MouseDown(object sender, MouseButtonEventArgs e) {
            if(spectrum.Visibility == Visibility.Visible) {
                spectrum.Visibility = Visibility.Collapsed;
                spectrumClose.Icon = FaxUi.MoonIcon.ArrowUp;
            }
            else {
                spectrum.Visibility = Visibility.Visible;
                spectrumClose.Icon = FaxUi.MoonIcon.ArrowDown;
            }
        }
        #endregion
    }
}
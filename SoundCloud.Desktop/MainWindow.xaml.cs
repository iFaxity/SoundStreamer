using Streamer.Net.SoundCloud;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        internal LikeFrame likeFrame;
        internal SettingsFrame settingsFrame;
        internal StreamFrame streamFrame;
        internal LocalFrame localFrame;

        DispatcherTimer timer;
        string menuSelected = "";
        #endregion

        // Login and Initialize Application UI
        public MainWindow() {
            // Loads all the settings
            AppSettings.Load();
            Player.Init(new System.Windows.Interop.WindowInteropHelper(this).Handle);

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
            playerPop.MouseDown += playerPop_MouseDown;
            playerPop.MouseLeave += playerPop_MouseLeave;
            playerBbar.MouseLeave += playerBbar_MouseLeave;
            playerBbar.MouseEnter += (sender, e) => playerPop.IsOpen = true;
            playerBbar.MouseMove += playerBbar_MouseMove;

            // Volume Popup Events
            playerVolPop.MouseLeave += playerVolPop_MouseLeave;
            playerVolSlider.ValueChanged += playerVolSlider_ValueChanged;
            playerVol.MouseDoubleClick += playerVol_MouseDoubleClick;
            playerVol.MouseEnter += playerVol_MouseEnter;
            playerVol.MouseLeave += playerVol_MouseLeave;

            // Player buttons
            playerPlay.MouseDown += playerPlay_MouseDown;
            playerPrev.MouseDown += playerPrev_MouseDown;
            playerNext.MouseDown += playerNext_MouseDown;

            // Search Box Events
            tbxSearch.KeyDown += (sender, e) => {
                if(e.Key == Key.Return)
                    contentFrame.Navigate(new SearchFrame(tbxSearch.Text));
            };
            tbxSearch.GotFocus += (sender, e) => tbxSearch.Text = "";
            tbxSearch.LostFocus += (sender, e) => {
                if(string.IsNullOrWhiteSpace(tbxSearch.Text))
                    tbxSearch.Text = "Search";
            };
            #endregion

            // Player Timer
            timer = new DispatcherTimer();
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

            // Initialise Shortcut Keys
            Shortcuts.Init(this);
            // Navigate to login page
            contentFrame.Navigate(new LoginPage(this));
            menuSelected = "stream";
        }

        #region Events
        public void Menu_Click(object sender, MouseEventArgs e) {
            // If not connected then dont try to change
            if(!SoundCloudClient.IsConnected)
                return;

            var t = (TextBlock)sender;
            if(menuSelected == t.Text.ToLower())
                return;

            menuSelected = t.Text.ToLower(); //Indicate which menu item is selected
            switch(menuSelected) {
                case "stream":
                    if(streamFrame == null) streamFrame = new StreamFrame();
                    contentFrame.Navigate(streamFrame);
                    return;
                case "likes":
                    if(likeFrame == null) likeFrame = new LikeFrame();
                    contentFrame.Navigate(likeFrame);
                    return;
                case "my tracks": return;
                case "settings":
                    if(settingsFrame == null) settingsFrame = new SettingsFrame();
                    contentFrame.Navigate(settingsFrame);
                    return;
                case "local":
                    if(localFrame == null) localFrame = new LocalFrame();
                    contentFrame.Navigate(localFrame);
                    return;
            }
        }
        public void Menu_Enter(object sender, MouseEventArgs e) {
            Cursor = Cursors.Hand;
            var t = sender as TextBlock;
            t.Padding = new Thickness(5, 0, 0, 0);
            t.Foreground = t.Foreground = FromHex("#FFC8C8C8");
        }
        public void Menu_Leave(object sender, MouseEventArgs e) {
            Cursor = Cursors.Arrow;
            var t = sender as TextBlock;
            t.Padding = new Thickness(0);
            t.Foreground = FromHex("#FFB7B7B7");
        }
        public void Player_MouseEnter(object sender, MouseEventArgs e) {
            var ico = sender as FaxUi.IcoMoon;
            ico.Foreground = FromHex("#FF666666"); // FFF5F5F5
        }
        public void Player_MouseLeave(object sender, MouseEventArgs e) {
            var ico = sender as FaxUi.IcoMoon;
            ico.Foreground = FromHex("#FFD3D3D3");
        }

        public void ChangeVolume(float volume) {
            playerVolSlider.Value = volume;
        }
        public void playerPlay_MouseDown(object sender, MouseButtonEventArgs e) {
            if(Player.State == BASSActive.BASS_ACTIVE_STOPPED)
                return;

            if(Player.State != BASSActive.BASS_ACTIVE_PLAYING)
                Player.Resume();
            else
                Player.Pause();

            if(Player.State == BASSActive.BASS_ACTIVE_PLAYING)
                playerPlay.Icon = FaxUi.MoonIcon.Pause;
            else
                playerPlay.Icon = FaxUi.MoonIcon.Play;
        }
        public void playerPrev_MouseDown(object sender, MouseButtonEventArgs e) {
            Player.Play(Player.TrackIndex - 1);
        }
        public void playerNext_MouseDown(object sender, MouseButtonEventArgs e) {
            Player.Play(Player.TrackIndex + 1);
        }

        public void Player_SongStarted(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                // Set track info and start the track
                var track = sender as Track;
                playerTotal.Text = track.Duration.ToTime();
                playerTitle.Text = track.Title;
                playerPlay.Icon = FaxUi.MoonIcon.Pause;
                Player.Volume = (float)playerVolSlider.Value;

                if(string.IsNullOrEmpty(track.ArtworkUrl)) {
                    playerImg.Source = null;
                    return;
                }
                // Get Track Image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(track.ArtworkUrl, UriKind.Absolute);
                bitmap.EndInit();
                playerImg.Source = bitmap;
            });
        }

        public void playerPop_MouseDown(object sender, MouseButtonEventArgs e) {
            if(Player.State == BASSActive.BASS_ACTIVE_STOPPED)
                return;

            var pos = e.GetPosition(playerPbar).X;
            Player.SetPos(playerPbar.Value = (pos / playerPbar.ActualWidth) * playerPbar.MaxValue);
        }
        public void playerPop_MouseLeave(object sender, MouseEventArgs e) {
            if(playerBbar.IsMouseOver)
                return;
            playerPop.IsOpen = false;
        }

        public void playerBbar_MouseLeave(object sender, MouseEventArgs e) {
            if(playerBbar.IsMouseOver)
                return;
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
        public void playerVol_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if(Player.Volume > 0f) {
                Player.Volume = 0f;
                playerVol.Icon = FaxUi.MoonIcon.VolumeMute;
            }
            else {
                Player.Volume = 1f;
                playerVol.Icon = FaxUi.MoonIcon.VolumeHigh;
            }
        }
        public void playerVol_MouseEnter(object sender, MouseEventArgs e) {
            playerVolPop.IsOpen = true;
            Player_MouseEnter(sender, e);
        }
        public void playerVol_MouseLeave(object sender, MouseEventArgs e) {
            if(playerVolPop.IsMouseOver)
                return;
            playerVolPop.IsOpen = false;
            Player_MouseLeave(sender, e);
        }

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

        Brush FromHex(string hex) {
            if(hex.Length == 9 && hex.StartsWith("#"))
                return new BrushConverter().ConvertFrom(hex) as Brush;
            else
                throw new Exception("Brush hex was invalid.");
        }
    }
}
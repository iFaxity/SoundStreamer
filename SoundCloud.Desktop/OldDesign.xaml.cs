using FaxUi;
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
using System.Windows.Shapes;

namespace SoundCloud.Desktop
{
    /// <summary>
    /// Interaction logic for OldDesign.xaml
    /// </summary>
    public partial class OldDesign : UiWindow
    {
        public OldDesign()
        {
            InitializeComponent();
        }
    }
}

/*Old Code

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Streamer.Net.SoundCloud;
using Un4seen.Bass;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace SoundCloud.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FaxUi.UiWindow
    {
        #region Properties
        //Application ID
        internal const string ClientID = @"589e0ff400a0bc83ecd8eb20b94b57de";
        internal const string ClientSecret = @"4194ded960527644e96fea6cedab671d";

        MeFrame meFrame;
        SearchFrame searchFrame;
        SettingsFrame settingsFrame;
        StreamFrame streamFrame;
        LocalFrame localFrame;

        DispatcherTimer timer;
        string menuSelected = "";
        #endregion

        //Login and Initialize Application UI
        public MainWindow()
        {
            new WindowDesign().ShowDialog();
            #region App Check
            //Checks if the application already runs
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if (p.Id != curr.Id && p.MainModule.FileName == curr.MainModule.FileName) Environment.Exit(0);
            }
            #endregion

            InitializeComponent();

            #region Player Events
            Player.Init(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            Player.SongEnded += (sender, e) => Player.Play(Player.TrackIndex + 1);

            //Update Player Title and Timer
            Player.SongStarted += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var track = sender as Track;
                    playerTotal.Text = track.Duration.ToTime();
                    playerTitle.Text = track.Title;
                    playerPlay.Icon = FaxUi.MoonIcon.Pause;
                });
            };
            #endregion

            #region Player Progressbar & Progressbar Popup Events
            playerPop.MouseDown += (sender, e) =>
            {
                if (Player.State == Un4seen.Bass.BASSActive.BASS_ACTIVE_STOPPED) return;

                var pos = e.GetPosition(playerPbar).X;
                Player.SetPos(playerPbar.Value = (pos / playerPbar.ActualWidth) * playerPbar.MaxValue);
            };

            //Indicator stuff
            playerPop.MouseLeave += (sender, e) => { if (playerBbar.IsMouseOver) return; playerPop.IsOpen = false; };
            playerBbar.MouseLeave += (sender, e) => { if (playerPop.IsMouseOver) return; playerPop.IsOpen = false; };
            playerBbar.MouseEnter += (sender, e) => { playerPop.IsOpen = true; };
            //playerPbar.MouseDown += (sender, e) =>
            //{
            //    if (Player.State == Un4seen.Bass.BASSActive.BASS_ACTIVE_STOPPED) return;

            //    var pos = e.GetPosition(playerPbar).X;
            //    Player.SetPos(playerPbar.Value = (pos / playerPbar.ActualWidth) * playerPbar.MaxValue);
            //};

            playerBbar.MouseMove += (sender, e) =>
            {
                var pos = e.GetPosition(playerPbar).X;

                playerPop.HorizontalOffset = pos - 14; //Offset is 14

                if (Player.State == Un4seen.Bass.BASSActive.BASS_ACTIVE_STOPPED) return;

                //playerPbar.Value = (pos / playerPbar.ActualWidth) * playerPbar.Maximum;

                var time = TimeSpan.FromSeconds((pos / playerPbar.ActualWidth) * Player.Tracks[Player.TrackIndex].Duration.TotalSeconds);
                playerPopTxt.Text = time.ToTime();
            };
            #endregion

            #region Volume Popup Events
            playerVol.MouseLeave += (sender, e) => { if (playerVolPop.IsMouseOver) return; playerVolPop.IsOpen = false; };
            playerVolPop.MouseLeave += (sender, e) => { if (playerVol.IsMouseOver) return; playerVolPop.IsOpen = false; };
            playerVolSlider.ValueChanged += (sender, e) =>
            {
                Player.Volume = (float)e.NewValue;

                if (e.NewValue > 0.75f) playerVol.Icon = FaxUi.MoonIcon.VolumeHigh;
                else if (e.NewValue > 0.5f) playerVol.Icon = FaxUi.MoonIcon.VolumeMedium;
                else if (e.NewValue > 0f) playerVol.Icon = FaxUi.MoonIcon.VolumeLow;
                else playerVol.Icon = FaxUi.MoonIcon.VolumeMute;
            };
            #endregion

            #region Player Timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += (sender, e) =>
            {
                if (Player.State == BASSActive.BASS_ACTIVE_STOPPED) return; //Return if nothing is happening
                playerBbar.Value = Player.GetBufferedPercent(); //Buffer Bar

                if (Player.State != BASSActive.BASS_ACTIVE_PLAYING) return; //Return if nothing is playing

                //Update Player Bar Progress
                var current = Player.GetPos();
                var total = Player.GetTotal();

                playerPbar.Value = (current.TotalSeconds / total.TotalSeconds) * playerPbar.MaxValue;
                playerCurrent.Text = current.ToTime(); //Set text of Current TimeSpan

                //Update Player button to Pause
                if (Player.State != BASSActive.BASS_ACTIVE_PLAYING) playerPlay.Icon = FaxUi.MoonIcon.Play;
            };
            timer.Start();
            #endregion

            Closing += (sender, e) => Bass.BASS_Free(); //Safely frees all the bass resources
            spectrumSize.MouseDown += (sender, e) =>
            {
                if (spectrum.BarCount > 32) { spectrum.BarCount = 32; spectrum.BarWidth = 10; spectrum.Height = 150; spectrumSize.Icon = FaxUi.MoonIcon.Expand2; }
                else { spectrum.BarCount = 64; spectrum.BarWidth = 20; spectrum.Height = 300; spectrumSize.Icon = FaxUi.MoonIcon.Collapse2; }
            };

            Shortcuts.Init(this); //Initialise Shortcut Keys
            contentFrame.Navigate(new LoginPage(contentFrame)); menuSelected = "stream";
        }

        #region Menu & Player Events
        private void Menu_Click(object sender, MouseEventArgs e)
        {
            foreach (UIElement u in Menu.Children)
            {
                if (u is TextBlock)
                {
                    ((TextBlock)u).Foreground = FromHex("#FFE1E1E1");
                    ((TextBlock)u).Margin = new Thickness(5);
                }
            }

            var t = (TextBlock)sender;

            //Do nice activated effect
            t.Foreground = FromHex("#FFFFFFFF");
            t.Margin = new Thickness(5, 10, 5, 5);

            if (menuSelected == t.Text.ToLower()) return;

            menuSelected = t.Text.ToLower(); //Indicate which menu item is selected
            if (!Streamer.Net.SoundCloud.SoundCloudClient.IsConnected) return;
            switch (menuSelected)
            {
                case "me":
                    if (meFrame == null) meFrame = new MeFrame();
                    contentFrame.Navigate(meFrame); 
                    return;
                case "search":
                    if (searchFrame == null) searchFrame = new SearchFrame();
                    contentFrame.Navigate(searchFrame);
                    return;
                case "settings":
                    if (settingsFrame == null) settingsFrame = new SettingsFrame();
                    contentFrame.Navigate(settingsFrame);
                    return;
                case "stream":
                    if (streamFrame == null) streamFrame = new StreamFrame();
                    contentFrame.Navigate(streamFrame);
                    return;

                case "local":
                    if (localFrame == null) localFrame = new LocalFrame();
                    contentFrame.Navigate(localFrame); 
                    return;
            }
        }
        private void Menu_Enter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            var t = (TextBlock)sender;

            if (((SolidColorBrush)t.Foreground).Color != Colors.White) t.Foreground = FromHex("#FFFFFFFF");
            else t.Foreground = t.Foreground = FromHex("#FFC8C8C8");
        }
        private void Menu_Leave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            var t = (TextBlock)sender;

            if (((SolidColorBrush)t.Foreground).Color != Colors.White) t.Foreground = FromHex("#FFFFFFFF");
            else t.Foreground = FromHex("#FFE1E1E1");
        }

        public void playerPlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.State == BASSActive.BASS_ACTIVE_STOPPED) return;

            if (Player.State != BASSActive.BASS_ACTIVE_PLAYING) Player.Resume();
            else Player.Pause();

            if (Player.State == Un4seen.Bass.BASSActive.BASS_ACTIVE_PLAYING) playerPlay.Icon = FaxUi.MoonIcon.Pause;
            else playerPlay.Icon = FaxUi.MoonIcon.Play;
        }
        public void playerPrev_MouseUp(object sender, MouseButtonEventArgs e) { Player.Play(Player.TrackIndex - 1); }
        public void playerNext_MouseUp(object sender, MouseButtonEventArgs e) { Player.Play(Player.TrackIndex + 1); }
        public void playerVol_MouseUp(object sender, MouseButtonEventArgs e) { Player.Volume = 0f; playerVol.Icon = FaxUi.MoonIcon.VolumeOff; }
        public void playerVol_MouseEnter(object sender, MouseEventArgs e)
        {
            playerVolPop.IsOpen = true;
        }
        #endregion

        Color BrushColor(SolidColorBrush b) { return new SolidColorBrush().Color; }
        Brush FromHex(string hex)
        {
            if (hex.Length == 9 && hex.StartsWith("#")) return new BrushConverter().ConvertFrom(hex) as Brush;
            else throw new Exception("Brush hex was invalid.");
        }
    }
}
*/

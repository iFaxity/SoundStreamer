using Streamer.Net.SoundCloud;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for CoverItem.xaml
    /// </summary>
    public partial class CoverItem : UserControl {
        public CoverItem(int trackId) {
            InitializeComponent();

            MouseEnter += (sender, e) => Background = new SolidColorBrush(Colors.LightGray);
            MouseLeave += (sender, e) => Background = null;

            var track = SoundCloudClient.Collection[trackId];
            if(!string.IsNullOrWhiteSpace(track.ArtworkUrl))
                img.Source = new BitmapImage(new Uri(track.GetCover(AlbumSize.x300),UriKind.Absolute));

            if(track.Private) locker.Visibility = Visibility.Visible;
            title.Text = track.Title;

            user.Text = track.User.Username;
            date.Text = track.Created.ToShortDateString();
            duration.Text = track.Duration.ToTime();

            // Download track
            //download.MouseDown += (sender, e) => Player.DownloadTrack(track);
            //download.MouseEnter += (sender, e) => download.Opacity = 1;
            //download.MouseLeave += (sender, e) => download.Opacity = 0.4;
        }
    }
}

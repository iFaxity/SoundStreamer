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

            var track = SoundCloudCore.Tracks[trackId];
            if(!string.IsNullOrWhiteSpace(track.ArtworkUrl))
                img.Source = new BitmapImage(new Uri(track.GetCover(AlbumSize.x300),UriKind.Absolute));

            if(track.Private)
                locker.Visibility = Visibility.Visible;
            title.Text = track.Title;

            user.Text = track.User.Username;
            date.Text = track.Created.ToShortDateString();
            duration.Text = track.Duration.ToTime();

            // Go to user profile
            //user.MouseDown += (sender, e) => {};
            //user.MouseEnter += (sender, e) => user.Opacity = 0.4;
            //user.MouseLeave += (sender, e) => user.Opacity = 1;

            // Show if liked or disliked
            if(track.Liked)
                like.Foreground = UICore.FromHex("#FFFF5500");

            // Like/Dislike this track
            like.MouseDown += (sender, e) => {
                if(Me.Like(track, track.Liked))
                    like.Foreground = UICore.FromHex(!track.Liked ? "#FFFF5500" : "##FF444444");
            };
            like.MouseEnter += (sender, e) => like.Opacity = 0.4;
            like.MouseLeave += (sender, e) => like.Opacity = 1;

            // Download this track
            //download.MouseDown += (sender, e) => Player.DownloadTrack(track);
            //download.MouseEnter += (sender, e) => download.Opacity = 0.4;
            //download.MouseLeave += (sender, e) => download.Opacity = 1;
        }

        public CoverItem(Playlist playlist) {

        }
    }
}

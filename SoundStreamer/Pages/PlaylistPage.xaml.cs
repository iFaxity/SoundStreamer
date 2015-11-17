using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Streamer.SoundCloud;

namespace SoundStreamer {
    /// <summary>
    /// Interaction logic for PlaylistFrame.xaml
    /// </summary>
    public partial class PlaylistPage : Page {
        public PlaylistPage(Playlist playlist) {
            InitializeComponent();

            Task.Factory.StartNew(() => {
                Core.ToggleSpinner(grid);

                for(int i = 0; i < playlist.Tracks.Count; i++) {
                    var t = playlist.Tracks[i];
                    Dispatcher.Invoke(() => {
                        var cover = new CoverItem(t);
                        cover.Margin = new Thickness(10, 0, 10, 10);
                        cover.Cursor = Cursors.Hand;
                        cover.MouseDown += (sender, e) => { Player.Tracks = playlist.Tracks; Player.Play(panel.Children.IndexOf((CoverItem)sender)); };
                        panel.Children.Add(cover);
                    });
                }

                Core.ToggleSpinner(grid);
            });
        }
    }
}
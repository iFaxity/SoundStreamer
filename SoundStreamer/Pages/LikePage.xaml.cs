using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Streamer.SoundCloud;

namespace SoundStreamer {
    /// <summary>
    /// Interaction logic for Me.xaml
    /// </summary>
    public partial class LikePage : Page {
        public LikePage() {
            InitializeComponent();

            scroll.ScrollChanged += (sender, e) => {
                var s = (ScrollViewer)sender;
                if(s.VerticalOffset >= (s.ScrollableHeight - 100) && !busy)
                    GetNext();
            };

            Refresh();
        }

        public static List<int> tracks = new List<int>();
        static bool busy = true;
        static int off = 0;

        // Inserts an entry anywhere in the view list
        void InsertCover(int index, Track track) {
            if(index < 0 || index > panel.Children.Count) {

            }
        }

        void AddCovers(List<int> list) {
            for(int i = 0; i < list.Count; i++) {
                if(tracks.Contains(list[i]))
                    continue;

                tracks.Add(list[i]);
                Dispatcher.Invoke(() => {
                    var cover = new CoverItem(list[i]);
                    cover.MouseDown += (sender, e) => {
                        if(Player.Tracks != tracks)
                            Player.Tracks = tracks;
                        Player.Play(panel.Children.IndexOf((CoverItem)sender));
                    };

                    panel.Children.Add(cover);
                });
            }
        }

        public void GetNext() {
            Task.Factory.StartNew(() => {
                busy = true;
                Core.ToggleSpinner(grid);
                // Add Track Covers
                AddCovers(Me.GetLikedTracks(off));
                off += 10;
                // Remove spinner
                Core.ToggleSpinner(grid);
                busy = false;
            });
        }

        public void Refresh() {
            // Clear items
            tracks.Clear();
            panel.Children.Clear();
            // Get next row of tracks
            GetNext();
        }
    }
}
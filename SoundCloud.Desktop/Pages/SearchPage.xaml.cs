using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Streamer.Net.SoundCloud;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class SearchPage : Page {
        public SearchPage(string q) {
            InitializeComponent();

            scroll.ScrollChanged += (sender, e) => {
                var s = sender as ScrollViewer;
                if(s.VerticalOffset >= (s.ScrollableHeight - 100) && offset < Me.LikesCount && !busy)
                    GetNext();
            };

            query = q;
            Refresh();
        }

        static List<int> tracks = new List<int>();
        static int offset = 0;
        static int length = 10;
        static bool busy = true;
        static string query;

        void AddCovers(List<int> list) {
            for(int i = 0; i < list.Count; i++) {
                tracks.Add(list[i]);
                Dispatcher.Invoke(() => {
                    var cover = new CoverItem(list[i]);
                    cover.Margin = new Thickness(10, 0, 10, 10);
                    cover.Cursor = Cursors.Hand;
                    cover.MouseDown += (sender, e) => { if(Player.Tracks != tracks) Player.Tracks = tracks; Player.Play(panel.Children.IndexOf((CoverItem)sender)); };

                    panel.Children.Add(cover);
                });
            }
            busy = false;
        }

        public void GetNext() {
            Task.Factory.StartNew(() => {
                busy = true;
                UICore.ToggleSpinner(grid);

                AddCovers(Track.Search(query, 0, length));
                offset += length;

                UICore.ToggleSpinner(grid);
                busy = false;
            });
        }
        public void Refresh() {
            // Clear items
            tracks.Clear();
            panel.Children.Clear();

            offset = 0;
            GetNext();
        }
    }
}

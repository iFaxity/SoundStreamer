using Streamer.Net.SoundCloud;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Stream.xaml
    /// </summary>
    public partial class StreamPage : Page {
        static List<int> tracks = new List<int>();
        static bool busy = true;

        public StreamPage() {
            InitializeComponent();

            scroll.ScrollChanged += (sender, e) => {
                var s = (ScrollViewer)sender;
                if(s.VerticalOffset >= (s.ScrollableHeight - 100) && !busy)
                    GetNext();
            };

            Refresh();
        }

        void AddCovers(List<int> list) {
            for(int i = 0; i < list.Count; i++) {
                tracks.Add(list[i]);
                Dispatcher.Invoke(() => {
                    var cover = new CoverItem(list[i]);
                    cover.Margin = new Thickness(10, 0, 10, 10);
                    cover.Cursor = Cursors.Hand;
                    cover.MouseDown += (sender, e) => {
                        if(Player.Tracks != tracks)
                            Player.Tracks = tracks;
                        Player.Play(panel.Children.IndexOf((CoverItem)sender));
                    };

                    panel.Children.Add(cover);
                });
            }
            busy = false;
        }

        public void GetNext() {
            Task.Factory.StartNew(() => {
                busy = true;
                UICore.ToggleSpinner(grid);

                AddCovers(Me.GetNextDashboard(DashboardType.All));

                UICore.ToggleSpinner(grid);
                busy = false;
            });
        }
        public void Refresh() {
            // Clear items
            tracks.Clear();
            panel.Children.Clear();

            GetNext();
        }
    }
}

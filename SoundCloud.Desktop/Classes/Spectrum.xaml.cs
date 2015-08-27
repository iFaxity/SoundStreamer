using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Threading;
using Un4seen.Bass;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public partial class Spectrum : UserControl {
        #region Dependencies
        public int BarWidth {
            get { return (int)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); UpdateBars(BarCount, value, this); }
        }
        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(int), typeof(Spectrum), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarWidthChanged)));
        private static void BarWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = sender as Spectrum;

            UpdateBars(source.BarCount, (int)e.NewValue, source);
        }

        public int BarCount {
            get { return (int)GetValue(BarCountProperty); }
            set { SetValue(BarCountProperty, value); UpdateBars(value, BarWidth, this); }
        }
        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(Spectrum), new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarCountChanged)));
        private static void BarCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = sender as Spectrum;

            UpdateBars((int)e.NewValue, source.BarWidth, source);
        }

        public Brush BarForeground {
            get { return (Brush)GetValue(BarForegroundProperty); }
            set { SetValue(BarForegroundProperty, value); }
        }
        public static readonly DependencyProperty BarForegroundProperty = DependencyProperty.Register("BarForeground", typeof(Brush), typeof(Spectrum), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#FFFF4800"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarForegroundChanged)));
        private static void BarForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = sender as Spectrum;

            source.panel.Background = (Brush)e.NewValue;
        }

        public Brush BarBackground {
            get { return (Brush)GetValue(BarBackgroundProperty); }
            set { SetValue(BarBackgroundProperty, value); }
        }
        public static readonly DependencyProperty BarBackgroundProperty = DependencyProperty.Register("BarBackground", typeof(Brush), typeof(Spectrum), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#FFBCBCBC"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarBackgroundChanged)));
        private static void BarBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = sender as Spectrum;
            foreach(Rectangle bar in source.panel.Children) bar.Fill = (Brush)e.NewValue;
        }

        private static void UpdateBars(int count, int width, Spectrum source) {
            source.panel.Children.Clear();
            source.Width = count * width;

            for(int i = 0; i < count; i++) {
                var bar = new Rectangle {
                    Fill = source.BarBackground,
                    Width = width,
                    Height = source.ActualHeight,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top
                };
                source.panel.Children.Add(bar);
            }
        }
        #endregion

        float[] data = new float[4096]; //Must be half of BASSData on GetData()

        public Spectrum() {
            InitializeComponent();

            Loaded += (sender, e) => {
                grid.Background = BarForeground;
                foreach(Rectangle bar in panel.Children) {
                    bar.Height = Height;
                    bar.Fill = BarBackground;
                }
                Start();
            };
            // Update bars when size changes
            SizeChanged += (sender, e) => {
                UpdateBars(BarCount, BarWidth, this);
            };
        }
        // Start the spectrum
        void Start() {
            var count = BarCount;
            var fft = new int[count];
            var spectrum = panel;

            Task.Factory.StartNew(async () => {
                while(true) {
                    try {
                        Dispatcher.Invoke(() => {
                            if(!IsEnabled || !IsVisible)
                                return;

                            if(count != BarCount) {
                                count = BarCount;
                                fft = new int[count];
                            }
                        });
                        // Check if the player is active
                        if(Player.channel == -1) {
                            await Task.Delay(100);
                            continue;
                        }

                        var active = true;
                        if(Player.State == BASSActive.BASS_ACTIVE_PLAYING)
                            fft = GetFFT(count);
                        else
                            active = false;

                        Dispatcher.Invoke(() => {
                            for(int i = 0; i < fft.Length; i++) {
                                var rect = panel.Children[i] as Rectangle;

                                if(!active && fft[i] < 0) {
                                    rect.Height = 0;
                                    continue;
                                }
                                else if(!active)
                                    fft[i] = (int)(fft[i] - (ActualHeight / 5));
                                // If FFT is below zero then make it 0
                                if(fft[i] < 0)
                                    fft[i] = 0;

                                rect.Height = fft[i] < 0 ? 0 : Height - (Height / 255) * fft[i];
                            }
                        });
                        // ~60 fps lock
                        await Task.Delay(1000 / 60);
                    }
                    catch {
                        continue;
                    }
                }
            });
        }
        // Gets FFT From raw data
        int[] GetFFT(int count) {
            int ret = Bass.BASS_ChannelGetData(Player.channel, data, (int)BASSData.BASS_DATA_FFT8192); //get channel fft data
            if(ret < -1)
                return new int[0];

            int x, y;
            int b0 = 0;

            var fft = new int[count];

            //Dont Touch the for iteration
            for(x = 0; x < count; x++)//computes the spectrum data, the code is taken from a bass_wasapi sample.
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (count - 1));
                if(b1 > 1023)
                    b1 = 1023;
                if(b1 <= b0)
                    b1 = b0 + 1;
                for(; b0 < b1; b0++) {
                    if(peak < data[1 + b0])
                        peak = data[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if(y > 255)
                    y = 255;
                if(y < 0)
                    y = 0;

                fft[x] = y;
            }

            return fft;
        }
    }
}

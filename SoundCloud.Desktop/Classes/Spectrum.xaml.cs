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
        /// <summary>
        /// Gets/Sets the inividual Bar Width
        /// </summary>
        public int BarWidth {
            get { return (int)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); UpdateBars(BarCount, value, this); }
        }
        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register("BarWidth", typeof(int), typeof(Spectrum), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarWidthChanged)));
        static void BarWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = (Spectrum)sender;
            UpdateBars(source.BarCount, (int)e.NewValue, source);
        }

        /// <summary>
        /// Gets/Sets the amount of Bars in the Spectrum
        /// </summary>
        public int BarCount {
            get { return (int)GetValue(BarCountProperty); }
            set { SetValue(BarCountProperty, value); UpdateBars(value, BarWidth, this); }
        }
        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register("BarCount", typeof(int), typeof(Spectrum), new FrameworkPropertyMetadata(16, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarCountChanged)));
        static void BarCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var source = (Spectrum)sender;
            UpdateBars((int)e.NewValue, source.BarWidth, source);
        }

        /// <summary>
        /// Gets/Sets the Brush of all bars
        /// </summary>
        public Brush BarForeground {
            get { return (Brush)GetValue(BarForegroundProperty); }
            set { SetValue(BarForegroundProperty, value); }
        }
        public static readonly DependencyProperty BarForegroundProperty = DependencyProperty.Register("BarForeground", typeof(Brush), typeof(Spectrum), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#FFFF4800"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarForegroundChanged)));
        static void BarForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            ((Spectrum)sender).spectrum.Background = (Brush)e.NewValue;
        }

        /// <summary>
        /// Gets/Sets the Brush of the Background
        /// </summary>
        public Brush BarBackground {
            get { return (Brush)GetValue(BarBackgroundProperty); }
            set { SetValue(BarBackgroundProperty, value); }
        }
        public static readonly DependencyProperty BarBackgroundProperty = DependencyProperty.Register("BarBackground", typeof(Brush), typeof(Spectrum), new FrameworkPropertyMetadata(new BrushConverter().ConvertFromString("#FFBCBCBC"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(BarBackgroundChanged)));
        static void BarBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            foreach(Rectangle bar in ((Spectrum)sender).spectrum.Children)
                bar.Fill = (Brush)e.NewValue;
        }

        // Called everytime something needs to be updated
        static void UpdateBars(int count, int width, Spectrum source) {
            source.spectrum.Children.Clear();
            source.Width = count * width;

            for(int i = 0; i < count; i++) {
                var bar = new Rectangle {
                    Fill = source.BarBackground,
                    Width = width,
                    Height = source.ActualHeight,
                    VerticalAlignment = VerticalAlignment.Top
                };
                source.spectrum.Children.Add(bar);
            }
        }
        #endregion

        // Must be half of BASSData on GetData()
        float[] data = new float[4096];
        /// <summary>
        /// Constructs a new Audio Spectrum
        /// </summary>
        public Spectrum() {
            InitializeComponent();

            Loaded += (sender, e) => {
                grid.Background = BarForeground;
                foreach(Rectangle bar in spectrum.Children) {
                    bar.Height = Height;
                    bar.Fill = BarBackground;
                }
                Start();
            };
            // Update bars when size changes
            SizeChanged += (sender, e) => UpdateBars(BarCount, BarWidth, this);
        }
        // Start the spectrum
        void Start() {
            // Initialize Variables
            var count = BarCount;
            var FFT = new int[count];

            // Run Spectrum outside the UI thread
            Task.Factory.StartNew(async() => {
                while(true) {
                    try {
                        // Check if bars should be updated
                        Dispatcher.Invoke(() => {
                            if(!IsEnabled || !IsVisible)
                                return;
                            if(count != BarCount) {
                                count = BarCount;
                                FFT = new int[count];
                            }
                        });
                        // Check if Player is Active
                        /*var active = true;
                        if(Player.State == BASSActive.BASS_ACTIVE_PLAYING)
                            FFT = GetFFT(count);
                        else
                            active = false;*/
                        if(Player.State == BASSActive.BASS_ACTIVE_PLAYING) {
                            FFT = GetFFT(count);
                            Dispatcher.Invoke(() => {
                                for(int i = 0; i < FFT.Length; i++) {
                                    var rect = (Rectangle)spectrum.Children[i];
                                    /*if(!active && FFT[i] < 0) {
                                        rect.Height = 0;
                                        continue;
                                    }
                                    else if(!active)
                                        FFT[i] = (int)(FFT[i] - (ActualHeight / 5));*/

                                    // If FFT is below zero then make it 0
                                    if(FFT[i] < 0)
                                        FFT[i] = 0;

                                    rect.Height = FFT[i] < 0 ? 0 : Height - (Height / 255) * FFT[i];
                                }
                            });
                        }
                        // ~60 fps lock
                        await Task.Delay(1000 / 60);
                    }
                    catch { }
                }
            });
        }

        // Calculates FFT (the peak between frequencies)
        int[] GetFFT(int count) {
            int ret = Bass.BASS_ChannelGetData(Player.channel, data, (int)BASSData.BASS_DATA_FFT8192); //get channel FFT data
            if(ret < -1)
                return new int[0];
            // Declare variables
            int x, y, b0 = 0;
            var FFT = new int[count];

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
                FFT[x] = y > 255 ? 255 : y < 0 ? 0 : y;
            }
            return FFT;
        }
    }
}

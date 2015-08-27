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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundCloud.Desktop {
    /// <summary>
    /// Interaction logic for Cover.xaml
    /// </summary>
    public partial class Cover : UserControl {
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); title.Text = value; }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Cover), new PropertyMetadata(null));

        public string User {
            get { return (string)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); user.Text = value; }
        }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(string), typeof(Cover), new PropertyMetadata(null));

        public string ImageUrl {
            get { return (string)GetValue(ImageUrlProperty); }
            set { SetValue(ImageUrlProperty, value); Update(value); }
        }
        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl", typeof(string), typeof(Cover), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ImageUrl_Changed)));

        private static void ImageUrl_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var sender = d as Cover;

            if(sender == null) return;

            sender.Update((string)e.NewValue);
        }

        void Update(string url) {
            if(string.IsNullOrEmpty(url)) return;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();

            img.Source = bitmap;
        }

        public Cover(string title = null, string user = null, string url = null) {
            InitializeComponent();

            Title = title;
            User = user;
            ImageUrl = url;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Streamer.Net.SoundCloud;

namespace SoundCloud.Desktop {
    public static class ExtensionMethods {
        public static string ToTime(this TimeSpan time) {
            return time.ToString(time.Hours > 0 ? @"hh\:mm\:ss" : @"mm\:ss");

            //string text = "{0}:{1}";

            //if (time.Minutes < 10) text.Replace("{0}", "0" + time.Minutes);
            //else text.Replace("{0}", "" + time.Minutes);

            //if (time.Seconds < 10) text.Replace("{1}", "0" + time.Seconds);
            //else text.Replace("{1}", "" + time.Seconds);

            //return text;
        }

        public static void Download(this Track track) {
            var wc = new WebClient();
            wc.DownloadFileCompleted += (sender, e) => { };
            wc.DownloadProgressChanged += (sender, e) => { };

            string file = track.ID + ".mp3";
            wc.DownloadFileAsync(new Uri(track.StreamUrl + "?client_id=589e0ff400a0bc83ecd8eb20b94b57de"), file);
        }
        /*public static BitmapImage GetImage(this Track track)
          {
              var bitmap = new BitmapImage();
              bitmap.BeginInit();
              bitmap.UriSource = new Uri(track.ArtworkUrl, UriKind.Absolute);
              bitmap.EndInit();

              return bitmap;
          }*/

        public static void Shuffle<T>(this IList<T> list) {
            Random rnd = new Random();

            for(int i = list.Count; i > 0; i--) {
                int n = rnd.Next(i);

                T value = list[n];
                list[n] = list[i];
                list[i] = value;
            }
        }
    }
}

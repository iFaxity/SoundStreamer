using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ionic.Zip;
using System.Windows;

namespace Installer {
    class Core {
        public static void Install(string path = "", string version = "latest") {
            var wc = new WebClient();
            wc.DownloadDataCompleted += (sender, e) => {
                if(e.Cancelled || e.Error != null)
                    return;

                using(var stream = new MemoryStream(e.Result)) {
                    using(var zip = ZipFile.Read(stream)) {
                        zip.ExtractAll(path, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            };
            wc.DownloadDataAsync(new Uri("http://scdesktop.us.to/download.php?v=" + version));
        }

        public static bool IsFileOpen(string file) {
            FileStream stream = null;
            try {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException) {
                return true;
            }
            finally {
                if(stream != null)
                    stream.Close();
            }
            return false;
        }

        public static void ErrorClose(string message, string title = "An error occurred. Installer will close.", bool close = true) {
            MessageBox.Show(message, title, MessageBoxButton.OK);
            Environment.Exit(0);
        }
    }
}

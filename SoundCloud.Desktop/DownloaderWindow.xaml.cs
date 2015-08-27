using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoundCloud.Desktop
{
    /// <summary>
    /// Interaction logic for Downloader.xaml
    /// </summary>
    public partial class DownloaderWindow : Window
    {
        public DownloaderWindow(string url, string path, string fileName)
        {
            InitializeComponent();

            Start(url, path, fileName);
        }

        void Start(string url, string path, string fileName)
        {
            var d = new Downloader(url, path, fileName);
            d.ProgressChanged += new EventHandler<DownloadProgressChanged>(ProgressChanged);
            d.Completed += new EventHandler<DownloadCompleted>(Completed);
        }

        private void ProgressChanged(object sender, DownloadProgressChanged e)
        {
            int mBytes = e.Bytes / 1000000;
            int totMBytes = e.BytesToReceive / 1000000;

            txtDownloaded.Text = mBytes + " mb / " + totMBytes + " mb";
            txtSpeed.Text = "Mbit: " + e.Speed / 1000000;
            Pbar.Value = e.Percent;
        }

        private void Completed(object sender, DownloadCompleted e)
        {
            MessageBox.Show("Completed");
        }
    }

    internal class DownloadCompleted : EventArgs
    {
        TimeSpan elapsedTime;
        public TimeSpan ElapsedTime { get { return elapsedTime; } set { elapsedTime = value; } }
        string state;
        public string State { get { return state; } set { state = value; } }
    }
    internal class DownloadProgressChanged : EventArgs
    {
        int bytes;
        public int Bytes { get { return bytes; } set { bytes = value; } }
        int bytesToReceive;
        public int BytesToReceive { get { return bytesToReceive; } set { bytesToReceive = value; } }
        int percent;
        public int Percent { get; set; }
        double speed;
        public double Speed { get; set; }
    }

    internal class Downloader
    {
        #region Variables & Properties
        string path;
        string fileName;

        WebClient webClient;
        Stopwatch sw = new Stopwatch();
        #endregion

        public Downloader(string url, string path)
        {
            //Change into right format
            path.Replace(@"/", @"\");
            //Verify Path
            int p = path.LastIndexOf(@"\");
            int s = path.LastIndexOf(".") - 1;

            this.fileName = path.Substring(p + 1, path.Length - s);
            this.path = path.Substring(0, p - 1);
            Setup(url);
        }
        public Downloader(string url, string path, string fileName)
        {
            Setup(url);
        }
        void Setup(string url)
        {
            // The variable that will be holding the url address (making sure it starts with http://)
            Uri uri = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(url) : new Uri("http://" + url);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (File.Exists(path + fileName)) File.Delete(path + fileName);

            //Start Download
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(_Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_ProgressChanged);

                // Start downloading the file
                try { sw.Start(); webClient.DownloadFileAsync(uri, path); }
                catch (Exception ex) { _Completed(this, new AsyncCompletedEventArgs(ex, true, this)); }
            }
        }

        #region Events
        public event EventHandler<DownloadCompleted> Completed;
        protected virtual void OnCompleted(DownloadCompleted e)
        {
            var handler = Completed;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<DownloadProgressChanged> ProgressChanged;
        protected virtual void OnProgressChanged(DownloadProgressChanged e)
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void _Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();
            var h = new DownloadCompleted();
            h.ElapsedTime = sw.Elapsed;

            if (e.Cancelled) h.State = "Cancelled";
            else if (Convert.ToBoolean(e.Error)) h.State = "Error";
            else h.State = "Completed";
            OnCompleted(h);
        }
        private void _ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var h = new DownloadProgressChanged();
            h.Bytes = (int)e.BytesReceived;
            h.BytesToReceive = (int)e.TotalBytesToReceive;
            h.Percent = e.ProgressPercentage;
            h.Speed = e.BytesReceived / sw.Elapsed.TotalSeconds;

            OnProgressChanged(h);
        }
        #endregion

        string GetAddress(string input)
        {
            string s = input.ToLower();
            int i = s.IndexOf('w');
            if (i > 3) i = 0;
            return "http://" + s.Substring(i, s.Length - i);
        }
    }

    /*class HttpDownloader
    {
        #region Variables
        private static string UrlLocation = null;
        private static string Url = null;
        private static string fileName = null;

        private static string downloadPath = null;

        private static string[] UrlList = null;
        private static string[] PathList = null;

        private static string extractPath = null;

        private static bool extractZips = false;
        private static bool isExtractable = false;
        private static bool extractToFolders = false;

        WebClient webClient;
        Stopwatch sw = new Stopwatch();
        #endregion

        //Starts the download
        private void StartDownload()
        {
            try
            {
                StartDownload(Url, UrlLocation);
            }
            catch
            {
                MessageBox.Show("Download of \"{0}\" failed", fileName);
            }

            //UrlList = Program.URLlist;
            //PathList = Program.URLpath;
            //int x = 0;
            //foreach (string s in UrlList)
            //{
            //    StartDownload(s, PathList[x]);
            //    x++;
            //}
        }
        //Finishes the file download
        private void Finish_Click(object sender, EventArgs e)
        {
            string loc = UrlLocation + fileName;
            try
            {
                int lastIndex = fileName.LastIndexOf('.');
                string foldername = fileName.Substring(0, (lastIndex - 1));

                MyExtract(loc, extractPath, foldername , fileName);
            }
            catch (ZipException ex) { MessageBox.Show("Extraction error. Exception: " + ex.Message); }
        }
        //Starts the downloading of the file
        public void StartDownload(string url, string location)
        {
            int iLastIndex = url.LastIndexOf('/');
            fileName = url.Substring(iLastIndex + 1, (url.Length - iLastIndex - 1));
            int urlDefine = url.IndexOf("://");
            int urlLength = url.Length - urlDefine;
            string tUrl = url.Substring(urlDefine, urlLength);
            tUrl = "http" + tUrl;

            if (!Directory.Exists(location)) Directory.CreateDirectory(location);
            if (File.Exists(location + @"\" + fileName)) File.Delete(location + @"\" + fileName);

            DownloadFile(url, location + @"\" + fileName);
        }
        //Downloads the file
        public void DownloadFile(string urlAddress, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with http://)
                //Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
                Uri URL = new Uri(urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    txtStatus.Text = ex.Message;
                }
            }
        }
        //The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            lblSpeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

            // Update the progressbar percentage only when the value is not the same.
            //Pbar.Value = e.ProgressPercentage;
            lblPercent.Text = Convert.ToString(e.ProgressPercentage) + @" %";

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            lblSpacer.Text = string.Format("{0} MB's / {1} MB's",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));

            // Show the percentage on our label.
            //lblPerc.Text = e.ProgressPercentage.ToString() + "%";
        }
        //The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (extractZips)
            {
                int lastIndex = fileName.LastIndexOf('.');
                string format = fileName.Substring(lastIndex, (fileName.Length - lastIndex - 1));
                if (format == ".zip" || format == ".rar" || format == ".7z" || format == ".gz" || format == ".jar") isExtractable = true;
            }

            if (e.Cancelled) MessageBox.Show("Download has been cancelled.");
            else if (Convert.ToBoolean(e.Error)) MessageBox.Show("Error: " + e.Error.Message);
            else { MessageBox.Show("Download completed!"); Finish.Visible = true; }
        }
        //Extracts the file. If it is a extractable file
        private static void MyExtract(string zipToUnpack, string unpackDirectory, string folderName, string fileName)
        {
            if (extractToFolders) unpackDirectory = unpackDirectory + folderName + fileName;
            using (ZipFile zip = ZipFile.Read(zipToUnpack))
            {
                // here, we extract every entry, but we could extract conditionally
                // based on entry name, size, date, checkbox status, etc.  
                foreach (ZipEntry e in zip) e.Extract(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
        }
        //Creates a zip file if one does not exist
        private static void CreateZip(string path, string saveDir)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(path);
                zip.Save(saveDir);
            }
        }
        //Add the files to the zip if there was one created
        private static void AddFiles(string path, string savePath, string filename)
        {
            string[] filenames = Directory.GetFiles(path);
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFiles(filenames);
                zip.Save(savePath + filename);
            }
        }
        //Updates the progress on the progressbar
        private void Downloader_SizeChanged(object sender, EventArgs e)
        {
            lblPercent.Location = new Point(Dbar.Width / 2 - 21 / 2 - 4, Dbar.Height / 2 - 15 / 2);
        }
    }*/
}

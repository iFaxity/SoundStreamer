using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading.Tasks;

using Streamer.Net.SoundCloud;
using Un4seen.Bass;

namespace SoundCloud.Desktop {
    public static class Player {
        #region Events
        public static event EventHandler SongStarted;
        static void OnSongStarted(Track track) {
            if(SongStarted != null)
                SongStarted(track, EventArgs.Empty);
        }

        public static event EventHandler SongEnded;
        static void OnSongEnded() {
            if(SongEnded != null)
                SongEnded(null, EventArgs.Empty);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The index of the active track
        /// </summary>
        public static int TrackIndex { get; set; }
        /// <summary>
        /// List of all queued tracks
        /// </summary>
        public static List<int> Tracks { get; set; }
        public static float Volume {
            get {
                var vol = 0f;
                Bass.BASS_ChannelGetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, ref vol);
                return vol;
            }
            set {
                Bass.BASS_ChannelSetAttribute(channel, BASSAttribute.BASS_ATTRIB_VOL, value);
            }
        }
        /// <summary>
        /// Gets the state of the Player
        /// </summary>
        public static BASSActive State {
            get {
                return Bass.BASS_ChannelIsActive(channel);
            }
        }

        static int _deviceIndex = 0;
        /// <summary>
        /// Sets the current audio device
        /// </summary>
        /// <param name="index">Audio device index</param>
        public static bool SetAudioDevice(int index) {
            _deviceIndex = index;
            return Bass.BASS_ChannelSetDevice(channel, index);
        }
        /// <summary>
        /// Get the active audio device
        /// </summary>
        public static BASS_DEVICEINFO AudioDevice {
            get {
                if(channel == -1)
                    return new BASS_DEVICEINFO();
                return AvailableDevices[Bass.BASS_ChannelGetDevice(channel)];
            }
        }
        /// <summary>
        /// List of all available audio devices
        /// </summary>
        public static List<BASS_DEVICEINFO> AvailableDevices {
            get {
                List<BASS_DEVICEINFO> list = new List<BASS_DEVICEINFO>();
                foreach(var device in Bass.BASS_GetDeviceInfos())
                    list.Add(device);
                return list;
            }
        }

        internal static int channel = -1;
        static string localTrackPath = @"Tracks\";
        static IntPtr _handle;
        #endregion

        #region Public Methods
        public static void Init(IntPtr handle = default(IntPtr)) {
            // Load Bass
            BassNet.Registration("izze96@gmail.com", "2X29392616152222");
            Bass.LoadMe(Utils.Is64Bit ? @"Bass\x64" : @"Bass\x86");

            _handle = handle;
            Volume = 1f;
            // Init all devices
            for(int i = 1; i < AvailableDevices.Count; i++)
                if(!Bass.BASS_Init(i, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle))
                    new Exception("Could not initialize device: " + AvailableDevices[i].id + " @Index: " + i);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, handle);
        }
        /// <summary>
        /// Stops the player
        /// </summary>
        public static bool Stop() { return Bass.BASS_ChannelStop(channel); }
        /// <summary>
        /// Plays/Resumes the player
        /// </summary>
        /// <param name="restart">Restart from the beginning</param>
        public static bool Resume(bool restart = false) { return Bass.BASS_ChannelPlay(channel, restart); }
        /// <summary>
        /// Pauses the player
        /// </summary>
        /// <returns></returns>
        public static bool Pause() { return Bass.BASS_ChannelPause(channel); }

        /// <summary>
        /// Sets the position of the player
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetPos(double value) {
            try {
                var sec = (value / 100) * SoundCloudClient.Collection[Tracks[TrackIndex]].Duration.TotalSeconds;
                var pos = Bass.BASS_ChannelSeconds2Bytes(channel, sec);
                Bass.BASS_ChannelSetPosition(channel, pos);
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// Gets the position of the player
        /// </summary>
        public static TimeSpan GetPos() {
            var pos = Bass.BASS_ChannelGetPosition(channel, BASSMode.BASS_POS_BYTES);
            return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(channel, pos));
        }
        /// <summary>
        /// Gets the total time of the current track
        /// </summary>
        public static TimeSpan GetTotal() {
            var len = Bass.BASS_ChannelGetLength(channel, BASSMode.BASS_POS_BYTES);
            return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(channel, len));
        }
        /// <summary>
        /// Gets the total buffered percentage
        /// </summary>
        public static double GetBufferedPercent() {
            var len = Bass.BASS_StreamGetFilePosition(channel, BASSStreamFilePosition.BASS_FILEPOS_END); // file/buffer length
            var buf = Bass.BASS_StreamGetFilePosition(channel, BASSStreamFilePosition.BASS_FILEPOS_BUFFER); // buffer level
            return buf * 100.0d / len; // percentage of buffer filled
        }

        /// <summary>
        /// Shuffles the queued tracks
        /// </summary>
        public static void Shuffle() {
            var rnd = new Random();
            for(int i = Tracks.Count; i > 0; i--) {
                int n = rnd.Next(i);

                var value = Tracks[n];
                Tracks[n] = Tracks[i];
                Tracks[i] = value;
            }
        }
        #endregion

        /// <summary>
        /// Plays track from queue index
        /// </summary>
        /// <param name="index">Track index</param>
        public static void Play(int index) {
            if(index < 0)
                TrackIndex = Tracks.Count - 1;
            else if(index >= Tracks.Count)
                TrackIndex = 0;
            else
                TrackIndex = index;
            // Play from track ID
            PlayTrack(Tracks[TrackIndex]);
        }

        static Task _task;
        /// <summary>
        /// Plays a track from a track ID
        /// </summary>
        /// <param name="trackId">ID of the track</param>
        public static void PlayTrack(int trackId) {
            if(_task != null)
                return;
            //await _lock.WaitAsync();
            _task = Task.Factory.StartNew(() => {
                // Check player state
                if(State != BASSActive.BASS_ACTIVE_STOPPED)
                    Bass.BASS_ChannelStop(channel);

                // Play local
                var path = localTrackPath + trackId + ".mp3";
                var track = SoundCloudClient.Collection[trackId];

                // If file exists locally then play it
                if(File.Exists(path))
                    channel = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);
                // Play From Stream
                else if(track.Streamable && track.StreamUrl != null)
                    channel = Bass.BASS_StreamCreateURL(track.StreamUrl, 0, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_STREAM_AUTOFREE, null, _handle);
                else // Error playing song. Call track end
                    OnSongEnded();

                // Set to call SongEnded Event when song has ended
                Bass.BASS_ChannelSetSync(channel, BASSSync.BASS_SYNC_END, 0, new SYNCPROC((h, c, d, u) => OnSongEnded()), IntPtr.Zero);
                Bass.BASS_Start();
                var res = Bass.BASS_ChannelPlay(channel, false);
                // Error when attemting to start song
                if(!res) {
                    MessageBox.Show(string.Format("Track '{0}' not playable. Error: {1}", track.Title, Bass.BASS_ErrorGetCode()), "Playback Error", MessageBoxButton.OK);
                    OnSongEnded();
                    return;
                }

                // Set audio device and invoke started event
                SetAudioDevice(_deviceIndex);
                OnSongStarted(track);
                _task = null;
                //_lock.Release();
            });
        }

        public static void DownloadTrack(int trackId) { DownloadTrack(SoundCloudClient.Collection[trackId]); }
        public static void DownloadTrack(Track track) {
            var wc = new WebClient();
            if(!Directory.Exists("Tracks"))
                Directory.CreateDirectory("Tracks");

            wc.DownloadFileAsync(new Uri(track.StreamUrl), "Tracks\\" + track.Title + ".mp3");
            //wc.DownloadFileCompleted += (wcS, wcE) => Process.Start("explorer.exe", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Tracks");
        }
    }
}

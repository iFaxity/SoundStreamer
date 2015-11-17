using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.SoundCloud {

    /// <summary>
    /// Class that represents a SoundCloud Playlist
    /// </summary>
    ////[System.Diagnostics.DebuggerStepThrough]
    [JsonConverter(typeof(PlaylistConverter))]
    public class Playlist {
        #region Properties
        public string Title { get; internal set; }
        public string ArtworkUrl { get; internal set; }
        public string Description { get; internal set; }
        public string Genre { get; internal set; }

        public int ID { get; internal set; }
        public int TracksCount { get; internal set; }

        public bool Streamable { get; internal set; }
        public bool Downloadable { get; internal set; }

        public User User { get; internal set; }
        public DateTime Created { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public List<int> Tracks { get; internal set; }
        #endregion

        /// <summary>
        /// Constructs a new empty Playlist
        /// </summary>
        public Playlist() { }

        /// <summary>
        /// Gets the album cover of the Track
        /// </summary>
        /// <param name="size">Size in pixels</param>
        /// <returns>The URL of the image</returns>
        public string GetCover(AlbumSize size = AlbumSize.x100) {
            return SoundCloudCore.ResolveCoverUrl(ArtworkUrl, size);
        }
        /// <summary>
        /// Searches for playlists that matches a filter
        /// </summary>
        /// <param name="q">Query string </param>
        /// <param name="startIndex">StartIndex of the request</param>
        /// <param name="length">Length of how many results to get</param>
        public static List<Playlist> Search(string q, int startIndex = 0, int length = 10) {
            // Get Response & Validate param
            return SoundCloudCore.SendRequest<List<Playlist>>(string.Format("playlists?q={0}&offset={1}&limit={2}", q, startIndex > 8000 ? 8000 : startIndex, length > 200 ? 200 : length));
        }
    }
}

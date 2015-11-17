using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Streamer.SoundCloud {
    /// <summary>
    /// Class that represents a SoundCloud Track
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    [JsonConverter(typeof(TrackConverter))]
    public class Track {
        #region Properties
        public string Title { get; set; }
        public string ArtworkUrl { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public string WaveformUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string StreamUrl { get; set; }

        public int ID { get; set; }
        public int Bpm { get; set; }
        public int Downloads { get; set; }
        public int Playbacks { get; set; }
        public int Likes { get; set; }

        public bool Streamable { get; set; }
        public bool Downloadable { get; set; }
        public bool Liked { get; set; }

        public bool Private { get; set; }
        public User User { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan Duration { get; set; }
        #endregion

        /// <summary>
        /// Constructs a new empty Track
        /// </summary>
        public Track() { }

        /// <summary>
        /// Gets the album cover of the Track
        /// </summary>
        /// <param name="size">Size in pixels</param>
        /// <returns>The URL of the image</returns>
        public string GetCover(AlbumSize size = AlbumSize.x100) {
            return SoundCloudCore.ResolveCoverUrl(ArtworkUrl, size);
        }

        /// <summary>
        /// Gets a Track from it's unique ID
        /// </summary>
        /// <param name="id">ID of track</param>
        public static int GetTrack(int id, bool reload = false) {
            // Check if this track already exists
            if(!reload) {
                if(SoundCloudCore.Tracks.ContainsKey(id))
                    return id;
            }
            // Get Track
            var track = SoundCloudCore.SendRequest<Track>("tracks/" + id + "?");
            // Add to collection
            SoundCloudCore.Tracks.Add(track.ID, track);
            return track.ID;
        }
        /// <summary>
        /// Searches for Tracks using a filter
        /// </summary>
        /// <param name="q">Query string</param>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many tracks to get</param>
        /// <param name="tags">Track tags</param>
        /// <param name="filter">Filter between 'all', 'public', 'private'</param>
        public static List<int> Search(string q, int startIndex = 0, int length = 10, string[] tags = null, string filter = "all") {
            return Search(q, startIndex, length, tags, filter, TimeSpan.MinValue, TimeSpan.MaxValue, DateTime.MinValue, DateTime.Now);
        }
        /// <summary>
        /// Searches for Tracks using a filter
        /// </summary>
        /// <param name="q">Query string</param>
        /// <param name="startIndex">StartIndex of response</param>
        /// <param name="length">Length of how many tracks to get</param>
        /// <param name="tags">Track tags</param>
        /// <param name="filter">Filter between 'all', 'public', 'private'</param>
        /// <param name="durationFrom">Track duration from</param>
        /// <param name="durationTo">Track duration to</param>
        /// <param name="createdFrom">Time created from</param>
        /// <param name="createdTo">Time created to</param>
        /// <param name="ids">A list of track ID's to filter on</param>
        /// <param name="genres">A list of genres</param>
        /// <param name="types">A list of types</param>
        /// <param name="license">A license to filter on</param>
        /// <param name="bpmFrom">Beats Per Minute from</param>
        /// <param name="bpmTo">Beats Per Minute from</param>
        /// <returns></returns>
        public static List<int> Search(string q, int startIndex, int length, string[] tags, string filter, TimeSpan durationFrom,
                                        TimeSpan durationTo, DateTime createdFrom, DateTime createdTo, int[] ids = null, string[] genres = null,
                                        string[] types = null, string license = null, int? bpmFrom = null, int? bpmTo = null) {
            var query = "tracks?";

            #region Old Code
            /*
            // Format Strings
            if(q != null)
                query += "q=" + q + "&";
            if(filter != null)
                query += "filter=" + filter + "&";
            if(license != null)
                query += "license=" + license + "&";
            // Format Integers
            if(bpmFrom != null)
                query += "bpm[from]=" + bpmFrom + "&";
            if(bpmTo != null)
                query += "bpm[to]=" + bpmTo + "&";
            if(startIndex > 0)
                query += "offset=" + (startIndex > 8000 ? 8000 : startIndex) + "&";
            if(length > 0)
                query += "limit=" + (length > 200 ? 200 : length) + "&";

            // Format Arrays
            if(tags != null && tags.Length > 0)
                query += "tags=" + string.Join(",", tags) + "&";
            if(ids != null && ids.Length > 0)
                query += "ids=" + string.Join(",", ids) + "&";
            if(genres != null && genres.Length > 0)
                query += "genres=" + string.Join(",", genres) + "&";
            if(types != null && types.Length > 0)
                query += "types=" + string.Join(",", types) + "&";

            // Format TimeSpans
            if(durationFrom != TimeSpan.MinValue)
                query += "duration[from]=" + durationFrom.TotalMilliseconds + "&";
            if(durationTo != TimeSpan.MaxValue)
                query += "duration[to]=" + durationTo.TotalMilliseconds + "&";

            // Format DateTime s
            if(createdFrom != DateTime.MinValue)
                query += "created_at[from]=" + createdFrom.ToString("yyyy-MM-dd hh:mm:ss") + "&";
            if(createdTo != DateTime.MaxValue)
                query += "created_at[to]=" + createdTo.ToString("yyyy-MM-dd hh:mm:ss") + "&";
            */
            #endregion

            // Format Strings
            query += q != null ? "q=" + q + "&" : "";
            query += filter != null ? "filter=" + filter + "&" : "";
            query += license != null ? "license=" + license + "&" : "";

            // Format Integers
            query += bpmFrom != null ? "bpm[from]=" + bpmFrom + "&" : "";
            query += bpmTo != null ? "bpm[to]=" + bpmTo + "&" : "";
            query += startIndex > 0 ? "offset=" + (startIndex > 8000 ? 8000 : startIndex) + "&" : "";
            query += length > 0 ? "limit=" + (length > 200 ? 200 : length) + "&" : "";

            // Format Arrays
            query += tags != null && tags.Length > 0 ? "tags=" + string.Join(",", tags) + "&" : "";
            query += ids != null && ids.Length > 0 ? "ids=" + string.Join(",", ids) + "&" : "";
            query += genres != null && genres.Length > 0 ? "genres=" + string.Join(",", genres) + "&" : "";
            query += types != null && types.Length > 0 ? "types=" + string.Join(",", types) + "&" : "";

            // Format TimeSpans
            query += durationFrom != TimeSpan.MinValue ? "duration[from]=" + durationFrom.TotalMilliseconds + "&" : "";
            query += durationTo != TimeSpan.MaxValue ? "duration[to]=" + durationTo.TotalMilliseconds + "&" : "";

            // Format DateTimes
            query += createdFrom != DateTime.MinValue ? "created_at[from]=" + createdFrom.ToString("yyyy-MM-dd hh:mm:ss") + "&" : "";
            query += createdTo != DateTime.MaxValue ? "created_at[to]=" + createdTo.ToString("yyyy-MM-dd hh:mm:ss") + "&" : "";

            // Get Response
            var res = SoundCloudCore.SendRequest<List<Track>>(query.Substring(0, query.Length - 1), HttpRequestMethod.GET);

            // Create a list of the tracks
            List<int> tracks = new List<int>();
            foreach(var track in res)
                tracks.Add(track.ID);

            return tracks;
        }
    }
}

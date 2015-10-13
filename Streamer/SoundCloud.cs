using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Streamer.Net.SoundCloud {
    /// <summary>
    /// Core SoundCloud API Features
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    public static class SoundCloudCore {
        #region Properties

        // Application POST info
        internal static string PostID {
            get {
                return "client_id=" + Login.ClientID;
            }
        }
        internal static string PostToken {
            get {
                return "oauth_token=" + AccessToken;
            }
        }

        #region Login & OAuth Properties

        /// <summary>
        /// The connection state of the current user
        /// </summary>
        public static bool IsConnected { get; private set; }
        /// <summary>
        /// Limit on how many results to get from pagination
        /// Min Value: 0; Max Value: 200
        /// </summary>
        public static int Limit {
            get {
                return limit;
            }
            set {
                limit = value < 0 ? 0 : (value > 200 ? 200 : value);
            }
        }
        static int limit = 10;

        /// <summary>
        /// Oauth2 Access Token
        /// </summary>
        public static string AccessToken { get; private set; }
        /// <summary>
        /// The defined scope (default = *)
        /// </summary>
        public static string Scope { get; private set; }
        /// <summary>
        /// Oauth2 Refresh Token
        /// </summary>
        public static string RefreshToken { get; private set; }
        /// <summary>
        /// Used in a request to use Pagination
        /// </summary>
        public static string Pagination {
            get {
                return "?linked_partitioning=1&limit=" + Limit;
            }
        }

        /// <summary>
        /// Current User Login info
        /// </summary>
        public static Login Login { get; private set; }
        /// <summary>
        /// AccessToken Expiring date
        /// </summary>
        public static DateTime Expires { get; private set; }
        #endregion

        #endregion

        /// <summary>
        /// Connects the user to SoundCloud
        /// </summary>
        /// <param name="login">Login information</param>
        public static bool Connect(Login login) {
            try {
                // Send Request to SoundCloud
                string url = string.Format("oauth2/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}", login.ClientID, login.ClientSecret, login.User, login.Pass);
                var res = SendTokenRequest<Dictionary<string, object>>(url, HttpRequestMethod.POST);

                // Format the returned token
                AccessToken = (string)res["access_token"];
                Expires = DateTime.Now.AddSeconds(Convert.ToDouble(res["expires_in"]));
                Scope = (string)res["scope"];
                RefreshToken = (string)res["refresh_token"];

                Login = login;
                IsConnected = true;
                // Call the logged in user to update its information
                Me.Refresh();
                // Reload the Tracks
                Tracks = new Dictionary<int, Track>();
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Contains all the tracks returned from any track query.
        /// This decreases memory usage by not duplicating tracks.
        /// </summary>
        public static Dictionary<int, Track> Tracks { get; private set; }
        /// <summary>
        /// Contains all the users returned from any user query.
        /// This decreases memory usage by not duplicating users.
        /// </summary>
        static Dictionary<int, User> Users { get; set; }

        // WIP
        static string RefreshAccessToken() {
            // Send Request
            string url = string.Format("oauth2/token&client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}", Login.ClientID, Login.ClientSecret, AccessToken);
            var res = SendTokenRequest<Dictionary<string, string>>(url, HttpRequestMethod.POST);

            // Format Response
            string text = "";
            foreach(KeyValuePair<string, string> pair in res)
                text = pair.Key + "=" + pair.Value + "\n";

            return text;
        }

        // Requests
        internal static T SendRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            // Base API URL
            if(!url.StartsWith("http"))
                url = "https://api.soundcloud.com/" + url;

            // Validates if you need token or client id to get properties
            if(url.Contains("/me")) {
                // Add Oauth Token to request
                if(!url.Contains("oauth_token"))
                    url += url.EndsWith("?") ? PostToken : "&" + PostToken;
            }
            else {
                // Authentication needed
                if(!IsConnected)
                    throw new Exception("Client not authenticated. Use 'SoundCloudCore.Connect(Login)' first");
                // Add Post ID to request
                url += url.EndsWith("?") ? PostID : "&" + PostID;
            }
            // Log this
            System.Diagnostics.Debug.WriteLine(url);
            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
        internal static T SendTokenRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest("https://api.soundcloud.com/" + url, method));
        }
        internal static T SendClientRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            if(!url.EndsWith("?"))
                url = url + "?" + PostID;
            else
                url += PostID;

            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }

        /// <summary>
        /// Resolves the artworks url to another size
        /// </summary>
        /// <param name="size">Size in pixels</param>
        /// <returns>The URL of the image</returns>
        internal static string ResolveCoverUrl(string url, AlbumSize size = AlbumSize.x100) {
            switch(size) {
                default:
                case AlbumSize.x100:
                    return url;
                case AlbumSize.x16:
                    return url.Replace("large.jpg", "mini.jpg");
                case AlbumSize.x18:
                case AlbumSize.x20:
                    return url.Replace("large.jpg", "tiny.jpg");
                case AlbumSize.x32:
                    return url.Replace("large.jpg", "small.jpg");
                case AlbumSize.x47:
                    return url.Replace("large.jpg", "badge.jpg");
                case AlbumSize.x67:
                    return url.Replace("large.jpg", "t67x67.jpg");
                case AlbumSize.x300:
                    return url.Replace("large.jpg", "t300x300.jpg");
                case AlbumSize.x400:
                    return url.Replace("large.jpg", "crop.jpg");
                case AlbumSize.x500:
                    return url.Replace("large.jpg", "t500x500.jpg");
            }
        }
    }

    /// <summary>
    /// Class that represents a SoundCloud Track
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    [JsonConverter(typeof(TrackConverter))]
    public class Track {
        #region Properties
        public string Title { get;  set; }
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

    /// <summary>
    /// Class that represents a SoundCloud Playlist
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
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

    /// <summary>
    /// Class that represents a SoundCloud User
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    public class User {
        #region Properties
        [JsonProperty("id")]
        public int ID { get; private set; }

        [JsonProperty("permalink")]
        public string Permalink { get; private set; }
        [JsonProperty("username")]
        public string Username { get; private set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; private set; }
        [JsonProperty("full_name")]
        public string Fullname { get; private set; }
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("website")]
        public string Website { get; private set; }

        [JsonProperty("online")]
        public bool Online { get; private set; }

        [JsonProperty("track_count")]
        public int TracksCount { get; private set; }
        [JsonProperty("playlist_count")]
        public int PlaylistsCount { get; private set; }
        [JsonProperty("followers_count")]
        public int FollowersCount { get; private set; }
        [JsonProperty("followings_count")]
        public int FollowingsCount { get; private set; }
        [JsonProperty("public_favorites_count")]
        public int LikesCount { get; private set; }
        #endregion

        /// <summary>
        /// Constructs a new empty User
        /// </summary>
        public User() { }

        /// <summary>
        /// Gets a Playlist from an index
        /// </summary>
        /// <param name="index">Playlist index</param>
        public Playlist GetPlaylist(int index) {
            var res = GetPlaylists(index, 1);

            if(res.Count != 1)
                throw new Exception("Playlist not found");
            return res[0];
        }
        /// <summary>
        /// Gets a Track ID from an index
        /// </summary>
        /// <param name="index">Track index</param>
        public int GetTrack(int index) {
            var res = GetTracks(index, 1);

            if(res.Count != 1)
                throw new Exception("Track not found");
            return res[0];
        }
        /// <summary>
        /// Gets a liked Track ID from an index
        /// </summary>
        /// <param name="index">Liked Track index</param>
        public int GetLikedTrack(int index) {
            var res = GetLikedTracks(index, 1);

            if(res.Count != 1)
                throw new Exception("Track not found");
            return res[0];
        }

        /// <summary>
        /// Gets a list of Playlists
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Playlists to get</param>
        public List<Playlist> GetPlaylists(int startIndex = 0, int length = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if(startIndex + length > PlaylistsCount) {
                startIndex = 0;
                length = PlaylistsCount - 1;
            }
            // Get Response
            return SoundCloudCore.SendRequest<List<Playlist>>(string.Format("users/{0}/playlists?offset={1}&limit={2}", ID, startIndex, length));
        }
        /// <summary>
        /// Gets a list of Tracks
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Tracks to get</param>
        public List<int> GetTracks(int startIndex = 0, int length = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if((startIndex + length) > TracksCount) {
                startIndex = 0;
                length = TracksCount - 1;
            }
            // Get Response
            var res = SoundCloudCore.SendRequest<List<Track>>(string.Format("users/{0}/tracks?offset={1}&limit={2}", ID, startIndex, length));

            List<int> list = new List<int>();
            foreach(var track in res)
                list.Add(track.ID);

            return list;
        }
        /// <summary>
        /// Gets a list of liked Tracks
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Tracks to get</param>
        public List<int> GetLikedTracks(int startIndex = 0, int length = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if((startIndex + length) > LikesCount) {
                startIndex = 0;
                length = LikesCount - 1;
            }
            // Get Response
            var res = SoundCloudCore.SendRequest<List<Track>>(string.Format("users/{0}/favorites?offset={1}&limit={2}", ID, startIndex, length));

            List<int> list = new List<int>();
            foreach(var track in res)
                list.Add(track.ID);
            return list;
        }

        /// <summary>
        /// Gets a User from it's unique ID
        /// </summary>
        /// <param name="id">ID of track</param>
        public static User GetUser(int id) {
            return SoundCloudCore.SendRequest<User>(string.Format("users/{0}?", id));
        }
        /// <summary>
        /// Searches for Users using a filter
        /// </summary>
        /// <param name="q">Query string</param>
        /// <param name="startIndex">StartIndex of response</param>
        /// <param name="length">Length of how many tracks to get</param>
        public static List<User> Search(string q, int startIndex = 0, int length = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            // Get Response
            return SoundCloudCore.SendRequest<List<User>>(string.Format("users?q={0}&offset={1}&limit={2}", q, startIndex, length));
        }
    }

    /// <summary>
    /// Class that represents the connected SoundCloud User
    /// </summary>
    //[System.Diagnostics.DebuggerStepThrough]
    public static class Me {
        #region Properties
        public static string Permalink { get; private set; }
        public static string Username { get; private set; }
        public static string AvatarUrl { get; private set; }
        public static string Fullname { get; private set; }
        public static string Description { get; private set; }
        public static string Website { get; private set; }

        public static int ID { get; private set; }
        public static int TracksCount { get; private set; }
        public static int PlaylistsCount { get; private set; }
        public static int FollowersCount { get; private set; }
        public static int FollowingsCount { get; private set; }
        public static int LikesCount { get; private set; }
        #endregion

        /// <summary>
        /// Refreshes the connected users info
        /// </summary>
        public static void Refresh() {
            var res = SoundCloudCore.SendRequest<Dictionary<string, object>>("me?");

            // Format Strings
            Username = (string)res["username"];
            Permalink = (string)res["permalink"];
            AvatarUrl = (string)res["avatar_url"] + "?" + SoundCloudCore.PostToken;
            Fullname = (string)res["full_name"];
            Description = (string)res["description"];
            Website = (string)res["website"];

            // Format Integers
            ID = Convert.ToInt32(res["id"]);
            TracksCount = Convert.ToInt32(res["track_count"]);
            PlaylistsCount = Convert.ToInt32(res["playlist_count"]);
            FollowersCount = Convert.ToInt32(res["followers_count"]);
            FollowingsCount = Convert.ToInt32(res["followings_count"]);
            LikesCount = Convert.ToInt32(res["public_favorites_count"]);
        }

        #region Public Methods
        /// <summary>
        /// Gets a Playlist from an index
        /// </summary>
        /// <param name="index">Playlist index</param>
        public static Playlist GetPlaylist(int index) {
            var res = GetPlaylists(index, 1);

            if(res.Count != 1)
                throw new Exception("Playlist not found");
            return res[0];
        }
        /// <summary>
        /// Gets a Track ID from an index
        /// </summary>
        /// <param name="index">Track index</param>
        public static Track GetTrack(int index) {
            var res = GetTracks(index, 1);

            if(res.Count != 1)
                throw new Exception("Track not found");
            return SoundCloudCore.Tracks[res[0]];
        }
        /// <summary>
        /// Gets a liked Track ID from an index
        /// </summary>
        /// <param name="index">Liked Track index</param>
        public static Track GetLikedTrack(int index) {
            var res = GetLikedTracks(index, 1);

            if(res.Count != 1)
                throw new Exception("Track not found");
            return SoundCloudCore.Tracks[res[0]];
        }

        /// <summary>
        /// Gets a list of Playlists
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Playlists to get</param>
        public static List<Playlist> GetPlaylists(int startIndex = 0, int limit = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            if((startIndex + limit) > PlaylistsCount) {
                startIndex = 0;
                limit = PlaylistsCount - 1;
            }
            // Get Response
            return SoundCloudCore.SendRequest<List<Playlist>>(string.Format("me/playlists?offset={0}&limit={1}", startIndex, limit));
        }
        /// <summary>
        /// Gets a list of Tracks
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Tracks to get</param>
        public static List<int> GetTracks(int startIndex = 0, int limit = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            if((startIndex + limit) > TracksCount) {
                startIndex = 0;
                limit = TracksCount - 1;
            }
            // Get Response
            var res = SoundCloudCore.SendRequest<List<Track>>(string.Format("me/tracks?offset={0}&limit={1}", startIndex, limit));

            List<int> list = new List<int>();
            foreach(var track in res)
                list.Add(track.ID);
            return list;
        }
        /// <summary>
        /// Gets a list of liked Tracks
        /// </summary>
        /// <param name="startIndex">Start index of search</param>
        /// <param name="length">Length of how many Tracks to get</param>
        public static List<int> GetLikedTracks(int startIndex = 0, int limit = 10) {
            // Validate Param
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            if((startIndex + limit) > LikesCount) {
                startIndex = 0;
                limit = LikesCount - 1;
            }
            // Get Response
            var res = SoundCloudCore.SendRequest<List<Track>>(string.Format("me/favorites?offset={0}&limit={1}", startIndex, limit));

            List<int> list = new List<int>();
            foreach(var track in res)
                list.Add(track.ID);
            return list;
        }

        // Add/Delete methods
        public static bool Like(Track track, bool dislike = true) {
            return Like(track.ID, dislike);
        }
        public static bool Like(int id, bool dislike = true) {
            try {
                SoundCloudCore.SendRequest<List<Track>>(string.Format("users/{0}/favorites/{1}", ID, id), dislike ? HttpRequestMethod.DELETE : HttpRequestMethod.PUT);
                //Track.Reload(id);
                return true;
            }
            catch {
                return false;
            }
        }
        #endregion

        #region Pagination (WIP)
        static string _playlistHref;
        /// <summary>
        /// Traverses down the pagination to get net set of Playlists
        /// </summary>
        /// <returns></returns>
        static List<Playlist> GetNextPlaylists() {
            // Buffer for returned playlists
            List<Playlist> list = new List<Playlist>();
            // Check if there is more to get
            if(_playlistHref == "stop")
                return list;

            var url = _playlistHref != null ? _playlistHref : "me/playlists" + SoundCloudCore.Pagination;

            // Get Response
            var res = SoundCloudCore.SendRequest<Dictionary<string, dynamic>>(url);
            _playlistHref = res.ContainsKey("next_href") ? _playlistHref = res["next_href"] : _playlistHref = "stop";

            foreach(var dict in res["collection"])
                list.Add(JsonConvert.DeserializeObject<Playlist>(dict.ToString()));
            return list;
        }

        static string _tracksHref;
        static List<int> GetNextTracks() {
            // Buffer for returned tracks
            List<int> list = new List<int>();
            // Check if there is more to get
            if(_tracksHref == "stop")
                return list;

            var url = _tracksHref != null ? _tracksHref : "me/tracks" + SoundCloudCore.Pagination;

            // Get Response
            var res = SoundCloudCore.SendRequest<Dictionary<string, dynamic>>(url);
            _tracksHref = res.ContainsKey("next_href") ? res["next_href"] : "stop";

            foreach(var dict in res["collection"])
                list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString())));
            return list;
        }

        static string _likeHref;
        static List<int> GetNextLiked() {
            // Buffer for returned tracks
            List<int> list = new List<int>();
            // Check if there is more to get
            if(_likeHref == "stop")
                return list;

            var url = _likeHref != null ? _likeHref : "me/favorites" + SoundCloudCore.Pagination;

            // Get Response
            var res= SoundCloudCore.SendRequest<Dictionary<string, dynamic>>(url);
            _likeHref = res.ContainsKey("next_href") ? res["next_href"] : "stop";

            foreach(var dict in res["collection"])
                list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString())));
            return list;
        }
        #endregion

        // Stream/Dashboard
        static string _dashboardHref;
        /// <summary>
        /// Gets the users 'Dashboard' aka 'Stream'
        /// </summary>
        /// <param name="type">The filtered type</param>
        /// <param name="tracksOnly"></param>
        /// <param name="startIndex">The index to begin from</param>
        /// <param name="length">The length of how many items to retrive</param>
        /// <returns></returns>
        public static List<int> GetNextDashboard(DashboardType type = DashboardType.All, bool tracksOnly = true) {
            // Buffer for returned activity
            List<int> list = new List<int>();

            // Check if there is more to get
            if(_dashboardHref == "stop")
                return list;

            // Set url variable
            string url = "";
            if(_dashboardHref != null)
                url = _dashboardHref;
            else {
                // Switch for what dashboard to get
                switch(type)
                {
                    default:
                    case DashboardType.All:
                        url = "me/activities";
                        break;
                    case DashboardType.Affiliated:
                        url = "me/activities/tracks/affiliated";
                        break;
                    case DashboardType.Exclusive:
                        url = "me/activities/tracks/exclusive";
                        break;
                    case DashboardType.Own:
                        url = "me/activities/all/own";
                        break;
                }
                url += SoundCloudCore.Pagination;
            }

            // Get Response
            var res = SoundCloudCore.SendRequest<Dictionary<string, dynamic>>(url);
            _dashboardHref = res.ContainsKey("next_href") ? res["next_href"] : "stop";

            foreach(var dict in res["collection"]) {
                if(tracksOnly && dict["type"].Value.Contains("track")) {
                    list.Add(JsonConvert.DeserializeObject<Track>(dict["origin"].ToString()).ID);
                    System.IO.File.AppendAllText("log.log", dict["origin"].ToString());
                }
            }
            return list;
        }
    }

    // Enumerators
    public enum DashboardType {
        /// <summary> Gets all recent activities </summary>
        All,
        /// <summary> Gets recent tracks from followed users. </summary>
        Affiliated,
        /// <summary> Gets recent exlusively shared tracks </summary>
        Exclusive,
        /// <summary> Gets recent activities from the current user. </summary>
        Own,
    }
    public enum AlbumSize {
        /// <summary> 16x16 jpg </summary>
        x16,
        /// <summary> 18x18 jpg (on avatars) </summary>
        x18,
        /// <summary> 20x20 jpg (on artworks) </summary>
        x20,
        /// <summary> 32x32 jpg </summary>
        x32,
        /// <summary> 47x47 jpg </summary>
        x47,
        /// <summary> 67x67 jpg (on artworks) </summary>
        x67,
        /// <summary> 100x100 jpg (default) </summary>
        x100,
        /// <summary> 300x300 jpg </summary>
        x300,
        /// <summary> 400x400 jpg </summary>
        x400,
        /// <summary> 500x500 jpg </summary>
        x500
    }

    // JSON Converters
    public class TrackConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(Track).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var json = serializer.Deserialize<Hashtable>(reader);
            var track = (Track)existingValue ?? new Track();
            // Strings
            track.Title = (string)json["title"];
            track.ArtworkUrl = (string)json["artwork_url"] + "?" + SoundCloudCore.PostID;
            track.Description = (string)json["description"];
            track.Genre = (string)json["genre"];
            track.WaveformUrl = (string)json["waveform_url"];
            track.DownloadUrl = (string)json["download_url"] + "?" + SoundCloudCore.PostID;
            track.StreamUrl = (string)json["stream_url"] + "?" + SoundCloudCore.PostID;
            track.Private = (string)json["sharing"] == "private";
            // Booleans
            track.Liked = Convert.ToBoolean(json["user_favorite"]);
            track.Downloadable = Convert.ToBoolean(json["downloadable"]);
            track.Streamable = Convert.ToBoolean(json["streamable"]);
            // Integers
            track.ID = Convert.ToInt32(json["id"]);
            track.Bpm =  Convert.ToInt32(json["bpm"]);
            track.Downloads = Convert.ToInt32(json["download_count"]);
            track.Playbacks = Convert.ToInt32(json["playback_count"]);
            track.Likes = Convert.ToInt32(json["favoritings_count"]);
            // Misc Types
            track.User = User.GetUser(Convert.ToInt32(json["user_id"]));
            track.Created = DateTime.Parse((string)json["created_at"]);
            track.Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(json["duration"]));

            // Add to collection
            SoundCloudCore.Tracks[track.ID] = track;
            return track;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotSupportedException();
        }
    }

    public class PlaylistConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(Track).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var json = serializer.Deserialize<Hashtable>(reader);
            // Load all tracks in this Playlist
            var list = JsonConvert.DeserializeObject<List<Track>>(json["tracks"].ToString());
            return new Playlist {
                Title = (string)json["title"],
                ArtworkUrl = (string)json["artwork_url"],
                Description = (string)json["description"],
                Genre = (string)json["genre"],

                ID = Convert.ToInt32(json["id"]),
                TracksCount = Convert.ToInt32(json["track_count"]),

                Streamable = (bool)json["streamable"],

                User = User.GetUser(Convert.ToInt32(json["user_id"])),
                Created = DateTime.Parse((string)json["created_at"]),
                Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(json["duration"])),
                Tracks = list.Select(x => x.ID).ToList(),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotSupportedException();
        }
    }
}
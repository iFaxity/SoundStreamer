using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.Net.SoundCloud {
    // Class for the Login and other Functions
    [System.Diagnostics.DebuggerStepThrough]
    public static class SoundCloudClient {
        #region Properties

        //Application ID POST info
        internal static string PostID { get { return @"client_id=" + Login.ClientID; } }
        internal static string PostToken { get { return @"oauth_token=" + AccessToken; } }

        //Login & AccessToken stuff

        /// <summary>
        /// Oauth2 Access Token
        /// </summary>
        public static string AccessToken { get; private set; }
        /// <summary>
        /// AccessToken Expiring date
        /// </summary>
        public static DateTime Expires { get; private set; }
        /// <summary>
        /// The defined scope (default = *)
        /// </summary>
        public static string Scope { get; private set; }
        /// <summary>
        /// Oauth2 Refresh Token
        /// </summary>
        public static string RefreshToken { get; private set; }
        /// <summary>
        /// Current User Login info
        /// </summary>
        public static Login Login { get; private set; }
        /// <summary>
        /// Limit on how many results to get from pagination
        /// </summary>
        public static int Limit {
            get { return limit; }
            set {
                if(value < 0) limit = 0;
                else if(value > 200) limit = 200;
                else limit = value;
            }
        }
        static int limit = 10;

        public static string Pagination { get { return @"?linked_partitioning=1&limit=" + SoundCloudClient.Limit; } }

        /// <summary>
        /// The connection state of the current user
        /// </summary>
        public static bool IsConnected { get { return connected; } private set { connected = value; } }
        static bool connected = false;

        #endregion

        /// <summary>
        /// Connects the user to SoundCloud
        /// </summary>
        /// <param name="login">Login information</param>
        public static bool Connect(Login login) {
            try {
                string url = string.Format(@"oauth2/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}", login.ClientID, login.ClientSecret, login.User, login.Pass);
                var response = SendTokenRequest<Dictionary<string, object>>(url, HttpRequestMethod.POST);

                //Format the returned token
                AccessToken = (string)response["access_token"];
                Expires = DateTime.Now.AddSeconds(Convert.ToDouble(response["expires_in"]));
                Scope = (string)response["scope"];
                RefreshToken = (string)response["refresh_token"];

                Login = login;
                IsConnected = true;
                Me.Refresh(); //Call the logged in user to update its information

                Collection = new Dictionary<int, Track>();
                return true;
            }
            catch { return false; }
        }
        /// <summary>
        /// Shuffles a list of tracks
        /// </summary>
        /// <param name="tracks">List of tracks to shuffle</param>
        public static List<int> Shuffle(List<int> tracks) {
            Random rnd = new Random();
            var list = tracks;
            for(var i = list.Count; i > 0; i--) {
                int n = rnd.Next(i), value = list[n];
                list[n] = list[i];
                list[i] = value;
            }
            return list;
        }
        /// <summary>
        /// Contains all the tracks returned from any track query.
        /// The reason for this is to decrease memory usage by not duplicating tracks.
        /// </summary>
        public static Dictionary<int, Track> Collection { get; private set; }

        // WIP
        static string RefreshAccessToken() {
            string url = string.Format(@"oauth2/token&client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}", Login.ClientID, Login.ClientSecret, AccessToken);

            var response = SendTokenRequest<Dictionary<string, string>>(url, HttpRequestMethod.POST);

            string text = "";
            foreach(KeyValuePair<string, string> pair in response) text = pair.Key + "=" + pair.Value + "\n";

            return text;
        }

        // Requests
        internal static T SendRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            if(!url.StartsWith("http")) url = "https://api.soundcloud.com/" + url; //Base API URL

            if(url.Contains("/me")) //Validates if you need token or client id to get properties
            {
                if(!url.Contains("oauth_token")) {
                    if(url.EndsWith("?")) url += PostToken;
                    else url += "&" + PostToken;
                }
            }
            else //Authentication needed
            {
                if(!IsConnected) throw new Exception("Client not authenticated");

                if(url.EndsWith("?")) url += PostID;
                else url += "&" + PostID;
            }

            System.Diagnostics.Debug.WriteLine(url);

            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
        internal static T SendTokenRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            url = "https://api.soundcloud.com/" + url;
            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
        internal static T SendClientRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET) {
            if(!url.EndsWith("?")) url = url + "?" + PostID;
            else url += PostID;

            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
    }

    // Track Class
    [System.Diagnostics.DebuggerStepThrough]
    public class Track {
        #region Properties
        public string Title { get; private set; }
        public string ArtworkUrl { get; private set; }
        public string Description { get; private set; }
        public string Genre { get; private set; }
        public string WaveformUrl { get; private set; }
        public string DownloadUrl { get; private set; }
        public string StreamUrl { get; private set; }

        public int ID { get; private set; }
        public int Bpm { get; private set; }
        public int Downloads { get; private set; }
        public int Playbacks { get; private set; }
        public int Likes { get; private set; }

        public bool Streamable { get; private set; }
        public bool Downloadable { get; private set; }
        public bool Liked { get; private set; }

        public bool Private { get; private set; }
        public User User { get; private set; }
        public DateTime Created { get; private set; }
        public TimeSpan Duration { get; private set; }
        #endregion

        public Track() { }
        public Track(int id) { }

        public static int GetTrack(int id) { return GetTrack(SoundCloudClient.SendRequest<Dictionary<string, object>>(@"tracks/" + id + "?")); }
        internal static int GetTrack(Dictionary<string, object> response) {
            if(response.Count < 1) throw new Exception("Error getting the track");

            var track = new Track();

            if(response.ContainsKey("title") && response["title"] != null) track.Title = (string)response["title"];
            if(response.ContainsKey("artwork_url") && response["artwork_url"] != null) track.ArtworkUrl = (string)response["artwork_url"] + "?" + SoundCloudClient.PostID;
            if(response.ContainsKey("description") && response["description"] != null) track.Description = (string)response["description"];
            if(response.ContainsKey("genre") && response["genre"] != null) track.Genre = (string)response["genre"];
            if(response.ContainsKey("waveform_url") && response["waveform_url"] != null) track.WaveformUrl = (string)response["waveform_url"];
            if(response.ContainsKey("download_url") && response["download_url"] != null) track.DownloadUrl = (string)response["download_url"] + "?" + SoundCloudClient.PostID;
            if(response.ContainsKey("stream_url") && response["stream_url"] != null) track.StreamUrl = (string)response["stream_url"] + "?" + SoundCloudClient.PostID;

            if(response.ContainsKey("user_favorite") && response["user_favorite"] != null) track.Liked = (bool)response["user_favorite"];
            if(response.ContainsKey("downloadable") && response["downloadable"] != null) track.Downloadable = (bool)response["downloadable"];
            if(response.ContainsKey("streamable") && response["streamable"] != null) track.Streamable = (bool)response["streamable"];

            if(response.ContainsKey("id") && response["id"] != null) track.ID = Convert.ToInt32(response["id"]);
            if(response.ContainsKey("bpm") && response["bpm"] != null) track.Bpm = Convert.ToInt32(response["bpm"]);
            if(response.ContainsKey("user_id") && response["user_id"] != null) track.User = User.GetUser(Convert.ToInt32(response["user_id"]));
            if(response.ContainsKey("created_at") && response["created_at"] != null) track.Created = DateTime.Parse((string)response["created_at"]);
            if(response.ContainsKey("duration") && response["duration"] != null) track.Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(response["duration"]));

            if(response.ContainsKey("sharing") && response["sharing"] != null) {
                if((string)response["sharing"] == "private") track.Private = true;
                else track.Private = false;
            }
            if(response.ContainsKey("download_count") && response["download_count"] != null) track.Downloads = Convert.ToInt32(response["download_count"]);
            if(response.ContainsKey("playback_count") && response["playback_count"] != null) track.Playbacks = Convert.ToInt32(response["playback_count"]);
            if(response.ContainsKey("favoritings_count") && response["favoritings_count"] != null) track.Likes = Convert.ToInt32(response["favoritings_count"]);

            if(!SoundCloudClient.Collection.ContainsKey(track.ID)) SoundCloudClient.Collection.Add(track.ID, track);
            return track.ID;
        }

        public string GetCover(AlbumSize size = AlbumSize.x100) {
            if(ArtworkUrl == null || !ArtworkUrl.Contains("large.jpg")) return ArtworkUrl;
            switch(size) {
                case AlbumSize.x16: return ArtworkUrl.Replace("large.jpg", "mini.jpg");
                case AlbumSize.x18: case AlbumSize.x20: return ArtworkUrl.Replace("large.jpg", "tiny.jpg");
                case AlbumSize.x32: return ArtworkUrl.Replace("large.jpg", "small.jpg");
                case AlbumSize.x47: return ArtworkUrl.Replace("large.jpg", "badge.jpg");
                case AlbumSize.x67: return ArtworkUrl.Replace("large.jpg", "t67x67.jpg");
                case AlbumSize.x300: return ArtworkUrl.Replace("large.jpg", "t300x300.jpg");
                case AlbumSize.x400: return ArtworkUrl.Replace("large.jpg", "crop.jpg");
                case AlbumSize.x500: return ArtworkUrl.Replace("large.jpg", "t500x500.jpg");
                default: return ArtworkUrl;
            }
        }

        #region Search
        public static List<int> Search(string q, int startIndex = 0, int length = 10, string[] tags = null, string filter = "all") { return Track.Search(q, startIndex, length, tags, filter, TimeSpan.MinValue, TimeSpan.MaxValue, DateTime.MinValue, DateTime.Now); }
        public static List<int> Search(string q, int startIndex, int length, string[] tags, string filter, TimeSpan durationFrom, TimeSpan durationTo, DateTime createdFrom, DateTime createdTo,
                                         int[] ids = null, string[] genres = null, string[] types = null, string license = null, int? bpmFrom = null, int? bpmTo = null) {
            string query = @"tracks?";
            //Format String
            if(q != null) query += "q=" + q + "&";
            if(filter != null) query += "filter=" + filter + "&";
            if(license != null) query += "license=" + license + "&";
            //Format Integer
            if(bpmFrom != null) query += "bpm[from]=" + bpmFrom + "&";
            if(bpmTo != null) query += "bpm[to]=" + bpmTo + "&";
            if(startIndex > 0) query += "offset=" + (startIndex > 8000 ? 8000 : startIndex) + "&";
            if(length > 0) query += "limit=" + (length > 200 ? 200 : length) + "&";
            //Format Array
            if(tags != null && tags.Length > 0) query += "tags=" + string.Join(",", tags) + "&";
            if(ids != null && ids.Length > 0) query += "ids=" + string.Join(",", ids) + "&";
            if(genres != null && genres.Length > 0) query += "genres=" + string.Join(",", genres) + "&";
            if(types != null && types.Length > 0) query += "types=" + string.Join(",", types) + "&";
            //Format TimeSpan
            if(durationFrom != TimeSpan.MinValue) query += "duration[from]=" + durationFrom.TotalMilliseconds + "&";
            if(durationTo != TimeSpan.MaxValue) query += "duration[to]=" + durationTo.TotalMilliseconds + "&";
            //Format DateTime 
            if(createdFrom != DateTime.MinValue) query += "created_at[from]=" + createdFrom.ToString("yyyy-MM-dd hh:mm:ss") + "&";
            if(createdTo != DateTime.MaxValue) query += "created_at[to]=" + createdTo.ToString("yyyy-MM-dd hh:mm:ss") + "&";

            query = query.Substring(0, query.Length - 1); //Remove last & char
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(query, HttpRequestMethod.GET);

            List<int> tracks = new List<int>();
            foreach(Dictionary<string, object> dict in response) tracks.Add(Track.GetTrack(dict));

            return tracks;
        }
        #endregion
    }

    // Playlist Class
    [System.Diagnostics.DebuggerStepThrough]
    public class Playlist {
        #region Properties
        public string Title { get; private set; }
        public string ArtworkUrl { get; private set; }
        public string Description { get; private set; }
        public string Genre { get; private set; }

        public int ID { get; private set; }
        public int TracksCount { get; private set; }

        public bool Streamable { get; private set; }
        public bool Downloadable { get; private set; }

        public User User { get; private set; }
        public DateTime Created { get; private set; }
        public TimeSpan Duration { get; private set; }
        public List<int> Tracks { get; private set; }
        #endregion

        public Playlist() { }
        public static Playlist GetPlaylist(int id) { return GetPlaylist(SoundCloudClient.SendRequest<Dictionary<string, object>>(@"tracks/" + id + "?")); }
        internal static Playlist GetPlaylist(Dictionary<string, object> response) {
            if(response.Count < 1) throw new Exception("Error getting the playlist");

            var playlist = new Playlist {
                Title = (string)response["title"],
                ArtworkUrl = (string)response["artwork_url"],
                Description = (string)response["description"],
                Genre = (string)response["genre"],

                ID = Convert.ToInt32(response["id"]),
                TracksCount = Convert.ToInt32(response["track_count"]),

                Streamable = (bool)response["streamable"],
                //Downloadable = (bool)response["downloadable"],

                User = User.GetUser(Convert.ToInt32(response["user_id"])),
                Created = DateTime.Parse((string)response["created_at"]),
                Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(response["duration"])),
                Tracks = new List<int>(),
            };

            var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response["tracks"].ToString());
            foreach(Dictionary<string, object> dict in list) playlist.Tracks.Add(Track.GetTrack(dict));

            return playlist;
        }

        public static List<Playlist> Search(string q, int startIndex = 0, int length = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"playlists?q={0}&offset={1}&limit={2}", q, startIndex, length));

            List<Playlist> list = new List<Playlist>();
            foreach(Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
    }

    // User Class
    [System.Diagnostics.DebuggerStepThrough]
    public class User {
        #region Properties
        public int ID { get; private set; }

        public string Permalink { get; private set; }
        public string Username { get; private set; }
        public string AvatarUrl { get; private set; }
        public string Fullname { get; private set; }
        public string Description { get; private set; }
        public string Website { get; private set; }

        public bool Online { get; private set; }

        public int TracksCount { get; private set; }
        public int PlaylistsCount { get; private set; }
        public int FollowersCount { get; private set; }
        public int FollowingsCount { get; private set; }
        public int LikesCount { get; private set; }
        #endregion

        public User() { }

        public static User GetUser(int id) { return GetUser(SoundCloudClient.SendRequest<Dictionary<string, object>>(string.Format(@"users/{0}?", id))); }
        internal static User GetUser(Dictionary<string, object> response) {
            if(response.Count < 1) throw new Exception("Error getting user");

            return new User {
                ID = Convert.ToInt32(response["id"]),

                Username = (string)response["username"],
                Permalink = (string)response["permalink"],
                AvatarUrl = (string)response["avatar_url"],
                Fullname = (string)response["full_name"],
                Description = (string)response["description"],
                Website = (string)response["website"],

                Online = (bool)response["online"],

                TracksCount = Convert.ToInt32(response["track_count"]),
                PlaylistsCount = Convert.ToInt32(response["playlist_count"]),
                FollowersCount = Convert.ToInt32(response["followers_count"]),
                FollowingsCount = Convert.ToInt32(response["followings_count"]),
                LikesCount = Convert.ToInt32(response["public_favorites_count"]),
            };
        }

        public static List<User> Search(string q, int startIndex = 0, int length = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users?q={0}&offset={1}&limit={2}", q, startIndex, length));

            List<User> list = new List<User>();
            foreach(Dictionary<string, object> dict in response) list.Add(User.GetUser(dict));

            return list;
        }

        public Playlist GetPlaylist(int index) {
            var response = GetPlaylists(index, 1);

            if(response.Count != 1) throw new Exception("Playlist not found");
            return response[0];
        }
        public int GetTrack(int index) {
            var response = GetTracks(index, 1);

            if(response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }
        public int GetLikedTrack(int index) {
            var response = GetLikedTracks(index, 1);

            if(response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }

        public List<Playlist> GetPlaylists(int startIndex = 0, int length = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if((startIndex + length) > PlaylistsCount) { startIndex = 0; length = PlaylistsCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/playlists?offset={1}&limit={2}", ID, startIndex, length));

            List<Playlist> list = new List<Playlist>();
            foreach(Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
        public List<int> GetTracks(int startIndex = 0, int length = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if((startIndex + length) > TracksCount) { startIndex = 0; length = TracksCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/tracks?offset={1}&limit={2}", ID, startIndex, length));

            List<int> list = new List<int>();
            foreach(Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));

            return list;
        }
        public List<int> GetLikedTracks(int startIndex = 0, int length = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if((startIndex + length) > LikesCount) { startIndex = 0; length = LikesCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/favorites?offset={1}&limit={2}", ID, startIndex, length));

            List<int> list = new List<int>();
            foreach(Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));
            return list;
        }
    }

    // Connected User Class
    [System.Diagnostics.DebuggerStepThrough]
    public static class Me {
        #region Properties
        public static int ID { get; private set; }

        public static string Permalink { get; private set; }
        public static string Username { get; private set; }
        public static string AvatarUrl { get; private set; }
        public static string Fullname { get; private set; }
        public static string Description { get; private set; }
        public static string Website { get; private set; }

        public static int TracksCount { get; private set; }
        public static int PlaylistsCount { get; private set; }
        public static int FollowersCount { get; private set; }
        public static int FollowingsCount { get; private set; }
        public static int LikesCount { get; private set; }
        #endregion

        public static void Refresh() {
            var response = SoundCloudClient.SendRequest<Dictionary<string, object>>(@"me?");

            ID = Convert.ToInt32(response["id"]);
            Username = (string)response["username"];
            Permalink = (string)response["permalink"];
            AvatarUrl = (string)response["avatar_url"] + "?" + SoundCloudClient.PostToken;
            Fullname = (string)response["full_name"];
            Description = (string)response["description"];
            Website = (string)response["website"];

            TracksCount = Convert.ToInt32(response["track_count"]);
            PlaylistsCount = Convert.ToInt32(response["playlist_count"]);
            FollowersCount = Convert.ToInt32(response["followers_count"]);
            FollowingsCount = Convert.ToInt32(response["followings_count"]);
            LikesCount = Convert.ToInt32(response["public_favorites_count"]);
        }

        // Public Methods
        public static Playlist GetPlaylist(int index) {
            var response = GetPlaylists(index, 1);

            if(response.Count != 1) throw new Exception("Playlist not found");
            return response[0];
        }
        public static Track GetTrack(int index) {
            var response = GetTracks(index, 1);

            if(response.Count != 1) throw new Exception("Track not found");
            return SoundCloudClient.Collection[response[0]];
        }
        public static Track GetLikedTrack(int index) {
            var response = GetLikedTracks(index, 1);

            if(response.Count != 1) throw new Exception("Track not found");
            return SoundCloudClient.Collection[response[0]];
        }

        public static List<Playlist> GetPlaylists(int startIndex = 0, int limit = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            if((startIndex + limit) > PlaylistsCount) { startIndex = 0; limit = PlaylistsCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/playlists?offset={0}&limit={1}", startIndex, limit));

            List<Playlist> list = new List<Playlist>();
            foreach(Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
        public static List<int> GetTracks(int startIndex = 0, int limit = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/tracks?offset={0}&limit={1}", startIndex, limit));

            List<int> list = new List<int>();
            foreach(Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));

            return list;
        }
        public static List<int> GetLikedTracks(int startIndex = 0, int limit = 10) {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            limit = limit > 200 ? 200 : limit;

            if((startIndex + limit) > LikesCount) { startIndex = 0; limit = LikesCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/favorites?offset={0}&limit={1}", startIndex, limit));

            List<int> list = new List<int>();
            foreach(Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));
            return list;
        }

        // Pagination (not yet working)
        static string PlaylistHref;
        static List<Playlist> GetNextPlaylists() {
            List<Playlist> list = new List<Playlist>(); //Buffer for returned tracks
            if(PlaylistHref == "stop") return list; // Check if there is more to get

            string url = "";
            if(PlaylistHref != null) url = PlaylistHref;
            else url = @"me/playlists" + SoundCloudClient.Pagination;

            var response = SoundCloudClient.SendRequest<Dictionary<string, dynamic>>(url);
            if(response.ContainsKey("next_href")) PlaylistHref = response["next_href"];
            else PlaylistHref = "stop";

            foreach(var dict in response["collection"]) list.Add(Playlist.GetPlaylist(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString())));
            return list;
        }

        static string TracksHref;
        static List<int> GetNextTracks() {
            List<int> list = new List<int>(); //Buffer for returned tracks
            if(TracksHref == "stop") return list; // Check if there is more to get

            string url = "";
            if(TracksHref != null) url = TracksHref;
            else url = @"me/tracks" + SoundCloudClient.Pagination;

            var response = SoundCloudClient.SendRequest<Dictionary<string, dynamic>>(url);
            if(response.ContainsKey("next_href")) TracksHref = response["next_href"];
            else TracksHref = "stop";

            foreach(var dict in response["collection"]) list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString())));
            return list;
        }

        static string LikeHref;
        static List<int> GetNextLiked() {
            List<int> list = new List<int>(); //Buffer for returned tracks
            if(LikeHref == "stop") return list; // Check if there is more to get

            string url = "";
            if(LikeHref != null) url = LikeHref;
            else url = @"me/favorites" + SoundCloudClient.Pagination;

            var response = SoundCloudClient.SendRequest<Dictionary<string, dynamic>>(url);
            if(response.ContainsKey("next_href")) LikeHref = response["next_href"];
            else LikeHref = "stop";

            foreach(var dict in response["collection"]) list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict.ToString())));
            return list;
        }

        // Stream/Dashboard
        static string DashboardHref;
        /// <summary>
        /// Gets the users 'Dashboard' or  'Stream'
        /// </summary>
        /// <param name="type">The filtered type</param>
        /// <param name="tracksOnly"></param>
        /// <param name="startIndex">The index to begin from</param>
        /// <param name="length">The length of how many items to retrive</param>
        /// <returns></returns>
        public static List<int> GetNextDashboard(DashboardType type = DashboardType.All, bool tracksOnly = true) {
            List<int> list = new List<int>(); //Buffer for returned tracks
            if(DashboardHref == "stop") return list; // Check if there is more to get

            string url = ""; // Set url variable
            if(DashboardHref != null) url = DashboardHref;
            else {
                switch(type) // Switch for what dashboard to get
                {
                    case DashboardType.All: url = @"me/activities"; break;
                    case DashboardType.Affiliated: url = @"me/activities/tracks/affiliated"; break;
                    case DashboardType.Exclusive: url = @"me/activities/tracks/exclusive"; break;
                    case DashboardType.Own: url = @"me/activities/all/own"; break;
                }
                url += SoundCloudClient.Pagination;
            }

            var response = SoundCloudClient.SendRequest<Dictionary<string, dynamic>>(url);
            if(response.ContainsKey("next_href")) DashboardHref = response["next_href"];
            else DashboardHref = "stop";

            foreach(var dict in response["collection"]) {
                if(tracksOnly && !dict["type"].Value.Contains("track")) continue;

                list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["origin"].ToString())));
            }
            return list;
        }

        // Add/Delete methods
        public static bool LikeTrack(Track track, bool add = true) { return LikeTrack(track.ID, add); }
        public static bool LikeTrack(int id, bool add = true) {
            try {
                var method = HttpRequestMethod.DELETE;
                if(add) method = HttpRequestMethod.PUT;

                SoundCloudClient.SendRequest<List<string>>(string.Format("users/{0}/favorites/{1}", ID, id), method);
                return true;
            }
            catch { return false; }
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
    public enum AlbumSize { x16, x18, x20, x32, x47, x67, x100, x300, x400, x500 }
}

/* Old Code
namespace Streamer.Net.SoundCloud
{
    // Class for the Login and other Functions
    public static class SoundCloudClient
    {
        #region Properties

        //Application ID POST info
        internal static string PostID { get { return @"client_id=" + Login.ClientID; } }
        internal static string PostToken { get { return @"oauth_token=" + AccessToken; } }

        //Login & AccessToken stuff

        /// <summary>
        /// Oauth2 Access Token
        /// </summary>
        public static string AccessToken { get; private set; }
        /// <summary>
        /// AccessToken Expiring date
        /// </summary>
        public static DateTime Expires { get; private set; }
        /// <summary>
        /// The defined scope (default = *)
        /// </summary>
        public static string Scope { get; private set; }
        /// <summary>
        /// Oauth2 Refresh Token
        /// </summary>
        public static string RefreshToken { get; private set; }
        /// <summary>
        /// Current User Login info
        /// </summary>
        public static Login Login { get; private set; }

        static bool connected = false;
        public static bool IsConnected { get { return connected; } private set { connected = value; } }

        #endregion

        /// <summary>
        /// Connects the user to SoundCloud
        /// </summary>
        /// <param name="login">Login information</param>
        public static bool Connect(Login login)
        {
            try
            {
                string url = string.Format(@"oauth2/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}", login.ClientID, login.ClientSecret, login.User, login.Pass);
                var response = SoundCloudClient.SendTokenRequest<Dictionary<string, object>>(url, HttpRequestMethod.POST);
                
                //Format the returned token
                AccessToken = (string)response["access_token"];
                Expires = DateTime.Now.AddSeconds(Convert.ToDouble(response["expires_in"]));
                Scope = (string)response["scope"];
                RefreshToken = (string)response["refresh_token"];

                Login = login;
                IsConnected = true;
                Me.Refresh(); //Call the logged in user to update its information

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Shuffles a list of tracks
        /// </summary>
        /// <param name="tracks">List of tracks to shuffle</param>
        public static List<Track> Shuffle(List<Track> tracks)
        {
            Random rnd = new Random();
            var list = tracks;

            for (int i = list.Count; i > 0; i--)
            {
                int n = rnd.Next(i);

                var value = list[n];
                list[n] = list[i];
                list[i] = value;
            }
            return list;
        }

        // WIP
        static string RefreshAccessToken()
        {
            string url = string.Format(@"oauth2/token&client_id={0}&client_secret={1}&grant_type=refresh_token&refresh_token={2}", Login.ClientID, Login.ClientSecret, AccessToken);

            var response = SoundCloudClient.SendTokenRequest<Dictionary<string, string>>(url, HttpRequestMethod.POST);

            string text = "";
            foreach (KeyValuePair<string, string> pair in response) text = pair.Key + "=" + pair.Value + "\n";

            return text;
        }
        
        // Requests
        internal static T SendRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET)
        {
            url = "https://api.soundcloud.com/" + url; //Base API URL
            if (url.Contains("/me")) //Validates if you need token or client id to get properties
            {
                if (url.EndsWith("?")) url += SoundCloudClient.PostToken;
                else url += "&" + SoundCloudClient.PostToken;
            }
            else //Authentication needed
            {
                if (!SoundCloudClient.IsConnected) throw new Exception("Client not authenticated");

                if (url.EndsWith("?")) url += SoundCloudClient.PostID;
                else url += "&" + SoundCloudClient.PostID;
            }

            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
        internal static T SendTokenRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET)
        {
            url = "https://api.soundcloud.com/" + url;
            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
        internal static T SendClientRequest<T>(string url, HttpRequestMethod method = HttpRequestMethod.GET)
        {
            if (!url.EndsWith("?")) url = url + "?" + SoundCloudClient.PostID;
            else url += SoundCloudClient.PostID;

            return JsonConvert.DeserializeObject<T>(HttpRequest.SendRequest(url, method));
        }
    }

    // Track Class
    public class Track
    {
        #region Properties
        public string Title { get; private set; }
        public string ArtworkUrl { get; private set; }
        public string Description { get; private set; }
        public string Genre { get; private set; }
        public string WaveformUrl { get; private set; }
        public string DownloadUrl { get; private set; }
        public string StreamUrl { get; private set; }

        public int ID { get; private set; }
        public int Bpm { get; private set; }
        public int Downloads { get; private set; }
        public int Playbacks { get; private set; }
        public int Likes { get; private set; }

        public bool Streamable { get; private set; }
        public bool Downloadable { get; private set; }
        public bool Liked { get; private set; }

        public bool Private { get; private set; }
        public User User { get; private set; }
        public DateTime Created { get; private set; }
        public TimeSpan Duration { get; private set; }
        #endregion

        public Track() { }
        public Track(int id) { }

        public Track GetTrack(int id) { return GetTrack(SoundCloudClient.SendRequest<Dictionary<string, object>>(@"tracks/" + id + "?")); }        
        internal static Track GetTrack(Dictionary<string,object> response)
        {
            if (response.Count < 1) throw new Exception("Error getting the track");

            var track = new Track();

            if (response.ContainsKey("title") && response["title"] != null) track.Title = (string)response["title"];
            if (response.ContainsKey("artwork_url") && response["artwork_url"] != null) track.ArtworkUrl = (string)response["artwork_url"] + "?" + SoundCloudClient.PostID;
            if (response.ContainsKey("description") && response["description"] != null) track.Description = (string)response["description"];
            if (response.ContainsKey("genre") && response["genre"] != null) track.Genre = (string)response["genre"];
            if (response.ContainsKey("waveform_url") && response["waveform_url"] != null) track.WaveformUrl = (string)response["waveform_url"];
            if (response.ContainsKey("download_url") && response["download_url"] != null) track.DownloadUrl = (string)response["download_url"] + "?" + SoundCloudClient.PostID;
            if (response.ContainsKey("stream_url") && response["stream_url"] != null) track.StreamUrl = (string)response["stream_url"] + "?" + SoundCloudClient.PostID;

            if (response.ContainsKey("user_favorite") && response["user_favorite"] != null) track.Liked = (bool)response["user_favorite"];
            if (response.ContainsKey("downloadable") && response["downloadable"] != null) track.Downloadable = (bool)response["downloadable"];
            if (response.ContainsKey("streamable") && response["streamable"] != null) track.Streamable = (bool)response["streamable"];

            if (response.ContainsKey("id") && response["id"] != null) track.ID = Convert.ToInt32(response["id"]);
            if (response.ContainsKey("bpm") && response["bpm"] != null) track.Bpm = Convert.ToInt32(response["bpm"]);
            if (response.ContainsKey("user_id") && response["user_id"] != null) track.User = User.GetUser(Convert.ToInt32(response["user_id"]));
            if (response.ContainsKey("created_at") && response["created_at"] != null) track.Created = DateTime.Parse((string)response["created_at"]);
            if (response.ContainsKey("duration") && response["duration"] != null) track.Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(response["duration"]));

            if (response.ContainsKey("sharing") && response["sharing"] != null) 
            {
                if(response["sharing"] == "private") track.Private = true;
                else track.Private = false;
            }
            if (response.ContainsKey("download_count") && response["download_count"] != null) track.Downloads = Convert.ToInt32(response["download_count"]);
            if (response.ContainsKey("playback_count") && response["playback_count"] != null) track.Playbacks = Convert.ToInt32(response["playback_count"]);
            if (response.ContainsKey("favoritings_count") && response["favoritings_count"] != null) track.Likes = Convert.ToInt32(response["favoritings_count"]);

            return track;
        }

        public string GetCover(AlbumSize size = AlbumSize.x100)
        {
            if (ArtworkUrl == null || !ArtworkUrl.Contains("large.jpg")) return ArtworkUrl;
            switch(size)
            {
                case AlbumSize.x16: return ArtworkUrl.Replace("large.jpg", "mini.jpg");
                case AlbumSize.x18: case AlbumSize.x20: return ArtworkUrl.Replace("large.jpg", "tiny.jpg");
                case AlbumSize.x32: return ArtworkUrl.Replace("large.jpg", "small.jpg");
                case AlbumSize.x47: return ArtworkUrl.Replace("large.jpg", "badge.jpg");
                case AlbumSize.x67: return ArtworkUrl.Replace("large.jpg", "t67x67.jpg");
                case AlbumSize.x300: return ArtworkUrl.Replace("large.jpg", "t300x300.jpg");
                case AlbumSize.x400: return ArtworkUrl.Replace("large.jpg", "crop.jpg");
                case AlbumSize.x500: return ArtworkUrl.Replace("large.jpg", "t500x500.jpg");
                default: return ArtworkUrl;
            }
        }

        #region Search
        public static List<Track> Search(string q, int startIndex = 0, int length = 10, string[] tags = null, string filter = "all") { return Track.Search(q, startIndex, length, tags, filter, TimeSpan.MinValue, TimeSpan.MaxValue, DateTime.MinValue, DateTime.Now); }
        public static List<Track> Search(string q, int startIndex, int length, string[] tags, string filter, TimeSpan durationFrom, TimeSpan durationTo, DateTime createdFrom, DateTime createdTo,
                                         int[] ids = null, string[] genres = null, string[] types = null, string license = null, int? bpmFrom = null, int? bpmTo = null)
        {
            string query = @"tracks?";
            //Format String
            if (q != null) query += "q=" + q + "&";
            if (filter != null) query += "filter=" + filter + "&";
            if (license != null) query += "license=" + license + "&";
            //Format Integer
            if (bpmFrom != null) query += "bpm[from]=" + bpmFrom + "&";
            if (bpmTo != null) query += "bpm[to]=" + bpmTo + "&";
            if (startIndex > 0) query += "offset=" + (startIndex > 8000 ? 8000 : startIndex) + "&";
            if (length > 0) query += "limit=" + (length > 200 ? 200 : length) + "&";
            //Format Array
            if (tags != null && tags.Length > 0) query += "tags=" + string.Join(",", tags) + "&";
            if (ids != null && ids.Length > 0) query += "ids=" + string.Join(",", ids) + "&";
            if (genres != null && genres.Length > 0) query += "genres=" + string.Join(",", genres) + "&";
            if (types != null && types.Length > 0) query += "types=" + string.Join(",", types) + "&";
            //Format TimeSpan
            if (durationFrom != TimeSpan.MinValue) query += "duration[from]=" + durationFrom.TotalMilliseconds + "&";
            if (durationTo != TimeSpan.MaxValue) query += "duration[to]=" + durationTo.TotalMilliseconds + "&";
            //Format DateTime 
            if (createdFrom != DateTime.MinValue) query += "created_at[from]=" + createdFrom.ToString("yyyy-MM-dd hh:mm:ss") + "&";
            if (createdTo != DateTime.MaxValue) query += "created_at[to]=" + createdTo.ToString("yyyy-MM-dd hh:mm:ss") + "&";

            query = query.Substring(0, query.Length - 1); //Remove last & char
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(query, HttpRequestMethod.GET);

            List<Track> tracks = new List<Track>();
            foreach(Dictionary<string, object> dict in response) tracks.Add(Track.GetTrack(dict));

            return tracks;
        }
        #endregion
    }

    // Playlist Class
    public class Playlist
    {
        #region Properties
        public string Title { get; private set; }
        public string ArtworkUrl { get; private set; }
        public string Description { get; private set; }
        public string Genre { get; private set; }

        public int ID { get; private set; }
        public int TracksCount { get; private set; }

        public bool Streamable { get; private set; }
        public bool Downloadable { get; private set; }

        public User User { get; private set; }
        public DateTime Created { get; private set; }
        public TimeSpan Duration { get; private set; }
        public List<Track> Tracks { get; private set; }
        #endregion

        public Playlist() { }
        public static Playlist GetPlaylist(int id) { return GetPlaylist(SoundCloudClient.SendRequest<Dictionary<string, object>>(@"tracks/" + id + "?")); }        
        internal static Playlist GetPlaylist(Dictionary<string,object> response)
        {
            if (response.Count < 1) throw new Exception("Error getting the playlist");

            var playlist = new Playlist
            {
                Title = (string)response["title"],
                ArtworkUrl = (string)response["artwork_url"],
                Description = (string)response["description"],
                Genre = (string)response["genre"],

                ID = Convert.ToInt32(response["id"]),
                TracksCount = Convert.ToInt32(response["track_count"]),

                Streamable = (bool)response["streamable"],
                //Downloadable = (bool)response["downloadable"],

                User = User.GetUser(Convert.ToInt32(response["user_id"])),
                Created = DateTime.Parse((string)response["created_at"]),
                Duration = TimeSpan.FromMilliseconds(Convert.ToInt32(response["duration"])),
                Tracks = new List<Track>(),
            };

            var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(response["tracks"].ToString());
            foreach(Dictionary<string, object> dict in list) playlist.Tracks.Add(Track.GetTrack(dict));

            return playlist;
        }

        public static List<Playlist> Search(string q, int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"playlists?q={0}&offset={1}&limit={2}", q, startIndex, length));

            List<Playlist> list = new List<Playlist>();
            foreach (Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
    }

    // User Class
    public class User
    {
        #region Properties
        public int ID { get; private set; }

        public string Permalink { get; private set; }
        public string Username { get; private set; }
        public string AvatarUrl { get; private set; }
        public string Fullname { get; private set; }
        public string Description { get; private set; }
        public string Website { get; private set; }

        public bool Online { get; private set; }

        public int TracksCount { get; private set; }
        public int PlaylistsCount { get; private set; }
        public int FollowersCount { get; private set; }
        public int FollowingsCount { get; private set; }
        public int LikesCount { get; private set; }
        #endregion

        public User() { }

        public static User GetUser(int id) { return GetUser(SoundCloudClient.SendRequest<Dictionary<string, object>>(string.Format(@"users/{0}?", id))); }     
        internal static User GetUser(Dictionary<string, object> response)
        {
            if (response.Count < 1) throw new Exception("Error getting user");

            return new User
            {
                ID = Convert.ToInt32(response["id"]),

                Username = (string)response["username"],
                Permalink = (string)response["permalink"],
                AvatarUrl = (string)response["avatar_url"],
                Fullname = (string)response["full_name"],
                Description = (string)response["description"],
                Website = (string)response["website"],

                Online = (bool)response["online"],

                TracksCount = Convert.ToInt32(response["track_count"]),
                PlaylistsCount = Convert.ToInt32(response["playlist_count"]),
                FollowersCount = Convert.ToInt32(response["followers_count"]),
                FollowingsCount = Convert.ToInt32(response["followings_count"]),
                LikesCount = Convert.ToInt32(response["public_favorites_count"]),
            };
        }

        public static List<User> Search(string q, int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users?q={0}&offset={1}&limit={2}", q, startIndex, length));

            List<User> list = new List<User>();
            foreach(Dictionary<string, object> dict in response) list.Add(User.GetUser(dict));

            return list;
        }

        public Playlist GetPlaylist(int index)
        {
            var response = GetPlaylists(index, 1);

            if (response.Count != 1) throw new Exception("Playlist not found");
            return response[0];
        }
        public Track GetTrack(int index)
        {
            var response = GetTracks(index, 1);

            if (response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }
        public Track GetLikedTrack(int index)
        {
            var response = GetLikedTracks(index, 1);

            if (response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }

        public List<Playlist> GetPlaylists(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > PlaylistsCount) { startIndex = 0; length = PlaylistsCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/playlists?offset={1}&limit={2}", ID, startIndex, length));

            List<Playlist> list = new List<Playlist>();
            foreach (Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
        public List<Track> GetTracks(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > TracksCount) { startIndex = 0; length = TracksCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/tracks?offset={1}&limit={2}", ID, startIndex, length));

            List<Track> list = new List<Track>();
            foreach (Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));

            return list;
        }
        public List<Track> GetLikedTracks(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > LikesCount) { startIndex = 0; length = LikesCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"users/{0}/favorites?offset={1}&limit={2}", ID, startIndex, length));

            List<Track> list = new List<Track>();
            foreach (Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));
            return list;
        }
    }

    // Connected User Class
    public static class Me
    {
        #region Properties
        public static int ID { get; private set; }

        public static string Permalink { get; private set; }
        public static string Username { get; private set; }
        public static string AvatarUrl { get; private set; }
        public static string Fullname { get; private set; }
        public static string Description { get; private set; }
        public static string Website { get; private set; }

        public static int TracksCount { get; private set; }
        public static int PlaylistsCount { get; private set; }
        public static int FollowersCount { get; private set; }
        public static int FollowingsCount { get; private set; }
        public static int LikesCount { get; private set; }
        #endregion

        public static void Refresh()
        {
            var response = SoundCloudClient.SendRequest<Dictionary<string, object>>(@"me?");

            ID = Convert.ToInt32(response["id"]);
            Username = (string)response["username"];
            Permalink = (string)response["permalink"];
            AvatarUrl = (string)response["avatar_url"] + "?" + SoundCloudClient.PostToken;
            Fullname = (string)response["full_name"];
            Description = (string)response["description"];
            Website = (string)response["website"];

            TracksCount = Convert.ToInt32(response["track_count"]);
            PlaylistsCount = Convert.ToInt32(response["playlist_count"]);
            FollowersCount = Convert.ToInt32(response["followers_count"]);
            FollowingsCount = Convert.ToInt32(response["followings_count"]);
            LikesCount = Convert.ToInt32(response["public_favorites_count"]);
        }

        // Public Methods
        public static Playlist GetPlaylist(int index)
        {
            var response = GetPlaylists(index, 1);

            if (response.Count != 1) throw new Exception("Playlist not found");
            return response[0];
        }
        public static Track GetTrack(int index)
        {
            var response = GetTracks(index, 1);

            if (response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }
        public static Track GetLikedTrack(int index)
        {
            var response = GetLikedTracks(index, 1);

            if (response.Count != 1) throw new Exception("Track not found");
            return response[0];
        }

        public static List<Playlist> GetPlaylists(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > PlaylistsCount) { startIndex = 0; length = PlaylistsCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/playlists?offset={0}&limit={1}", startIndex, length));

            List<Playlist> list = new List<Playlist>();
            foreach (Dictionary<string, object> dict in response) list.Add(Playlist.GetPlaylist(dict));

            return list;
        }
        public static List<Track> GetTracks(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > TracksCount) { startIndex = 0; length = TracksCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/tracks?offset={0}&limit={1}", startIndex, length));

            List<Track> list = new List<Track>();
            foreach (Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));

            return list;
        }
        public static List<Track> GetLikedTracks(int startIndex = 0, int length = 10)
        {
            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;

            if ((startIndex + length) > LikesCount) { startIndex = 0; length = LikesCount - 1; }
            var response = SoundCloudClient.SendRequest<List<Dictionary<string, object>>>(string.Format(@"me/favorites?offset={0}&limit={1}", startIndex, length));

            List<Track> list = new List<Track>();
            foreach (Dictionary<string, object> dict in response) list.Add(Track.GetTrack(dict));
            return list;
        }

        /// <summary>
        /// Gets the users 'Dashboard' or  'Stream'
        /// </summary>
        /// <param name="type">The filtered type</param>
        /// <param name="tracksOnly"></param>
        /// <param name="startIndex">The index to begin from</param>
        /// <param name="length">The length of how many items to retrive</param>
        /// <returns></returns>
        public static List<Track> GetDashboard(DashboardType type = DashboardType.All, bool tracksOnly = true, int startIndex = 0, int length = 10)
        {
            string address = "";

            switch (type) //Switch for what dashboard to get
            {
                case DashboardType.All: address = @"me/activities"; break;
                case DashboardType.Affiliated: address = @"me/activities/tracks/affiliated"; break;
                case DashboardType.Exclusive: address = @"me/activities/tracks/exclusive"; break;
                case DashboardType.Own: address = @"me/activities/all/own"; break;
            }

            startIndex = startIndex > 8000 ? 8000 : startIndex;
            length = length > 200 ? 200 : length;
            var response = SoundCloudClient.SendRequest<Dictionary<string, dynamic>>(string.Format(address + @"?offset={0}&limit={1}", startIndex, length));

            List<Track> list = new List<Track>(); //Buffer for returned tracks

            var collection = response["collection"];
            foreach (var dict in collection)
            {
                var t = dict["type"];
                if (tracksOnly && !t.Value.Contains("track")) continue;

                var origin = dict["origin"];
                list.Add(Track.GetTrack(JsonConvert.DeserializeObject<Dictionary<string, object>>(origin.ToString())));
            }
            return list;
        }

        //Add methods
        public static bool LikeTrack(Track track, bool add = true) { return LikeTrack(track.ID, add); }
        public static bool LikeTrack(int id, bool add = true)
        {
            try
            {
                var method = HttpRequestMethod.DELETE;
                if (add) method = HttpRequestMethod.PUT; 

                SoundCloudClient.SendRequest<List<string>>(string.Format("users/{0}/favorites/{1}", ID, id), method);
                return true;
            }
            catch { return false; }
        }
    }

    // Enumerators
    public enum DashboardType
    {
        /// <summary> Gets all recent activities </summary>
        All,
        /// <summary> Gets recent tracks from followed users. </summary>
        Affiliated,
        /// <summary> Gets recent exlusively shared tracks </summary>
        Exclusive,
        /// <summary> Gets recent activities from the current user. </summary>
        Own,
    }
    public enum AlbumSize { x16, x18, x20, x32, x47, x67, x100, x300, x400, x500 }
}
 */

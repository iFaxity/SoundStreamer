using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.SoundCloud {
    /// <summary>
    /// Class that represents the connected SoundCloud User
    /// </summary>
    ////[System.Diagnostics.DebuggerStepThrough]
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
                SoundCloudCore.SendRequest<object>("me/favorites/" + id + "?", dislike ? HttpRequestMethod.DELETE : HttpRequestMethod.PUT);
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
            var res = SoundCloudCore.SendRequest<Dictionary<string, dynamic>>(url);
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
                switch(type) {
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
}

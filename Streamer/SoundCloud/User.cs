using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.SoundCloud {
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
}

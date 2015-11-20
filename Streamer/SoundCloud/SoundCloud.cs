using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Streamer.SoundCloud {
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
                var url = string.Format("oauth2/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}", login.ClientID, login.ClientSecret, login.User, login.Pass);
                var res = SendTokenRequest<Dictionary<string, object>>(url, HttpRequestMethod.POST);

                // Format the returned token
                AccessToken = (string)res["access_token"];
                Expires = DateTime.Now.AddSeconds(Convert.ToDouble(res["expires_in"]));
                Scope = (string)res["scope"];
                RefreshToken = (string)res["refresh_token"];

                Me.Refresh();
                // Set Properties
                Login = login;
                IsConnected = true;
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
                    throw new Exception("Client not authenticated. Use 'SoundCloudCore..Connect(Login)' first");
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
            url += !url.EndsWith("?") ? "?" + PostID : PostID;
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
}
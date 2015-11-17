using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace Streamer {
    public enum HttpRequestMethod { GET, PUT, POST, DELETE }
    [System.Diagnostics.DebuggerStepThrough]
    public struct Login {
        [JsonProperty("user")]
        public string User { get; private set; }
        [JsonProperty("pass")]
        public string Pass { get; private set; }
        [JsonIgnore]
        public string ClientID { get; private set; }
        [JsonIgnore]
        public string ClientSecret { get; private set; }

        /// <summary>
        /// Constructs a new OAuth Login
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="pass">Password</param>
        /// <param name="clientId">OAuth client id</param>
        /// <param name="clientSecret">OAuth client secret</param>
        public Login(string user, string pass, string clientId, string clientSecret) {
            User = user;
            Pass = pass;
            ClientID = clientId;
            ClientSecret = clientSecret;
        }

        /// <summary>
        /// Sets Client ID & Client Secret
        /// </summary>
        /// <param name="clientId">OAuth client id</param>
        /// <param name="clientSecret">OAuth client secret</param>
        public void SetClientInfo(string clientId, string clientSecret) {
            ClientID = clientId;
            ClientSecret = clientSecret;
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public static class HttpRequest {
        static WebRequest GetJsonRequest(string url, HttpRequestMethod method) {
            var request = WebRequest.Create(url);

            request.Method = Enum.GetName(typeof(HttpRequestMethod), method).ToUpper();
            request.ContentType = "application/json";
            request.ContentLength = 0;

            return request;
        }

        public static string SendRequest(string url, HttpRequestMethod method, int count = 3) {
            Exception ex = null;
            for(int i = 0; i < count; i++) //Try sending 3 Times
            {
                var request = GetJsonRequest(url, method);
                try {
                    var response = (HttpWebResponse)request.GetResponse();
                    if(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) {
                        using(var stream = new StreamReader(response.GetResponseStream()))
                            return stream.ReadToEnd();
                    }
                    else if(response.StatusCode == HttpStatusCode.InternalServerError)
                        count--;
                    else
                        throw new Exception(string.Format("Error while sending request. Status Code: {0} ({1})", response.StatusCode, Enum.GetName(typeof(HttpStatusCode), response.StatusCode)));
                }
                catch(Exception e) {
                    ex = e;
                }
            }
            throw ex;
        }
    }
}

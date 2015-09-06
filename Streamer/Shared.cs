using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.Net {
    internal enum HttpRequestMethod { GET, PUT, POST, DELETE }
    //[System.Diagnostics.DebuggerStepThrough]
    public class Login {
        public string User { get; private set; }
        public string Pass { get; private set; }
        [JsonIgnore]
        public string ClientID { get; private set; }
        [JsonIgnore]
        public string ClientSecret { get; private set; }

        public Login(string user, string pass, string clientId, string clientSecret) {
            User = user;
            Pass = pass;
            ClientID = clientId;
            ClientSecret = clientSecret;
        }

        public void SetClientInfo(string clientId, string clientSecret) { ClientID = clientId; ClientSecret = clientSecret; }
    }
    [System.Diagnostics.DebuggerStepThrough]
    internal class HttpRequest {
        static WebRequest GetJsonRequest(string url, HttpRequestMethod method) {
            var request = WebRequest.Create(url);

            request.Method = Enum.GetName(typeof(HttpRequestMethod), method).ToUpper();
            request.ContentType = "application/json";
            request.ContentLength = 0;

            return request;
        }

        internal static string SendRequest(string url, HttpRequestMethod method, int count = 3) {
            Exception ex = null;
            for(int i = 0; i < count; i++) //Try sending 3 Times
            {
                var request = GetJsonRequest(url, method);
                try {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) {
                        using(var stream = new StreamReader(response.GetResponseStream())) return stream.ReadToEnd();
                    }
                    else if(response.StatusCode == HttpStatusCode.InternalServerError) count--;
                }
                catch(Exception e) { ex = e; }
            }
            throw ex;
        }
    }
}

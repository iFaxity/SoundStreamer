using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamer.SoundCloud {
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
            track.Bpm = Convert.ToInt32(json["bpm"]);
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

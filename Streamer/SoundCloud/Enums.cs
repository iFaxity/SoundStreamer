namespace Streamer.SoundCloud {
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
}

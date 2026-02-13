using System.Collections.Generic;

namespace MyMusicPlayer.Models
{
    public class ItunesTrackInfo
    {
        public string? TrackName { get; set; }
        public string? ArtistName { get; set; }
        public string? AlbumName { get; set; }
        public string? ArtworkUrl { get; set; }

        public List<string> CustomImages { get; set; } = new List<string>();
    }
}

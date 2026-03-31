namespace TVTrack.Models
{
    public class TVShow
    {
        // Class for single tv show, containing api data, title, overview, poster, etc
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public string? FirstAirDate { get; set; }
        public int? NumberOfSeasons { get; set; }

        public ICollection<ShowLog> ShowLogs { get; set; } = new List<ShowLog>();
        public ICollection<WatchList> WatchListEntries { get; set; } = new List<WatchList>();
        public ICollection<CustomListItem> CustomListItems { get; set; } = new List<CustomListItem>();
    }
}

namespace TVTrack.Models
{
    public class TVShow
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public string? FirstAirDate { get; set; }
        public int? NumberOfSeasons { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<WatchList> WatchListEntries { get; set; } = new List<WatchList>();
        public ICollection<CustomListItem> CustomListItems { get; set; } = new List<CustomListItem>();
    }
}

namespace TVTrack.Models.ViewModels
{
    public class ShowViewModel
    {
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? PosterUrl { get; set; }
        public double? AvgRating { get; set; }
        public bool IsInWatchlist { get; set; }
        public List<ReviewItemViewModel> Reviews { get; set; } = new List<ReviewItemViewModel>();
    }
}

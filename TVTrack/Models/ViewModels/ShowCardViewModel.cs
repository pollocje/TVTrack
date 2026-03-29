namespace TVTrack.Models.ViewModels
{
    public class ShowCardViewModel
    {
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
    }
}

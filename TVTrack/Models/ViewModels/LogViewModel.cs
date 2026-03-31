namespace TVTrack.Models.ViewModels
{
    public class LogViewModel
    {
        // IMPORTANT VIEW MODEL
        // used for intaking log entry data when user marks a show as watched
        public int TmdbId { get; set; }
        public LogType LogType { get; set; } = LogType.Series;
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? EpisodeTitle { get; set; }
        public int? Rating { get; set; }
        public string? Review { get; set; }
        public DateTime WatchedOn { get; set; } = DateTime.Today;
    }
}

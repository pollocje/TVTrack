namespace TVTrack.Models.ViewModels
{
    public class LogEntryViewModel
    {
        // IMPORTANT VIEW MODEL
        // used for showing the log entries of a show in the details page
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public LogType LogType { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? EpisodeTitle { get; set; }
        public int? Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime LoggedAt { get; set; }

        public string LogLabel => LogType switch
        {
            LogType.Episode when SeasonNumber != null && EpisodeNumber != null =>
                $"S{SeasonNumber}E{EpisodeNumber}" + (string.IsNullOrEmpty(EpisodeTitle) ? "" : $" \"{EpisodeTitle}\""),
            LogType.Season when SeasonNumber != null => $"Season {SeasonNumber}",
            _ => "Series"
        };
    }
}

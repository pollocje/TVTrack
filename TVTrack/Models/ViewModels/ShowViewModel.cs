namespace TVTrack.Models.ViewModels
{
    public class ShowViewModel
    {
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? PosterPath { get; set; }
        public string? PosterUrl { get; set; }
        public string? FirstAirDate { get; set; }
        public int? NumberOfSeasons { get; set; }
        public double? AvgRating { get; set; }
        public bool IsInWatchlist { get; set; }
        public List<ShowLogItemViewModel> RecentLogs { get; set; } = new();
        public List<CustomListSummary> UserLists { get; set; } = new();
    }

    public class CustomListSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ShowLogItemViewModel
    {
        public string Username { get; set; } = string.Empty;
        public LogType LogType { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? EpisodeTitle { get; set; }
        public int? Rating { get; set; }
        public string? Review { get; set; }
        public DateTime WatchedOn { get; set; }

        public string LogLabel => LogType switch
        {
            LogType.Episode when SeasonNumber != null && EpisodeNumber != null =>
                $"S{SeasonNumber}E{EpisodeNumber}" + (string.IsNullOrEmpty(EpisodeTitle) ? "" : $" \"{EpisodeTitle}\""),
            LogType.Season when SeasonNumber != null => $"Season {SeasonNumber}",
            _ => "Series"
        };
    }
}

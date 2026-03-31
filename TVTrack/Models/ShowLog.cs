namespace TVTrack.Models
{
    public enum LogType { Series, Season, Episode }

    public class ShowLog
    {
        // Class for the log object of a show
        // occurs when user marks a show as watched
        // They can rate, review, and choose between series, episode, or season log types
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ShowId { get; set; }
        public LogType LogType { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? EpisodeTitle { get; set; }
        public int? Rating { get; set; }
        public string? Review { get; set; }
        public DateTime WatchedOn { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public TVShow Show { get; set; } = null!;
    }
}

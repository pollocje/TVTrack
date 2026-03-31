namespace TVTrack.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }

        public List<ShowCardViewModel> Watchlist { get; set; } = new();
        public List<ListSummaryViewModel> Lists { get; set; } = new();
        public List<LogEntryViewModel> RecentLogs { get; set; } = new();
    }

    public class RatedShowViewModel
    {
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterUrl { get; set; }
        public int Score { get; set; }
    }
}

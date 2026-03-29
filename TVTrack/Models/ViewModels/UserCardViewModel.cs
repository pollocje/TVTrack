namespace TVTrack.Models.ViewModels
{
    public class UserCardViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int FollowersCount { get; set; }
    }
}

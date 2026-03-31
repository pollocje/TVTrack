namespace TVTrack.Models.ViewModels
{
    public class FollowListViewModel
    {
        // Small view model for showing list of followers or users following
        public string Username { get; set; } = string.Empty;
        public string ListType { get; set; } = string.Empty; // "Followers" or "Following"
        public List<UserCardViewModel> Users { get; set; } = new List<UserCardViewModel>();
    }
}

namespace TVTrack.Models.ViewModels
{
    public class UserSearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<UserCardViewModel> Results { get; set; } = new();
    }
}

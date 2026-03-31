namespace TVTrack.Models.ViewModels
{
    public class ListViewModel
    {
        // View model for actual list, shows name, description, sharetoken,
        // and shows in the list itself
        public int ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ShareToken { get; set; }
        public List<ShowCardViewModel> Shows { get; set; } = new List<ShowCardViewModel>();
    }
}

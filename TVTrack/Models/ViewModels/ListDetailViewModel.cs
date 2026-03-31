namespace TVTrack.Models.ViewModels
{
    public class ListDetailViewModel
    {
        public int ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public List<ShowCardViewModel> Shows { get; set; } = new();
    }
}

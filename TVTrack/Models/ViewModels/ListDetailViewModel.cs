namespace TVTrack.Models.ViewModels
{
    public class ListDetailViewModel
    {
        // View model for showing list details
        // ID, name, description, owner of list
        public int ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public string? ShareToken { get; set; }
        public List<ShowCardViewModel> Shows { get; set; } = new();
    }
}

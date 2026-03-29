namespace TVTrack.Models.ViewModels
{
    public class ListSummaryViewModel
    {
        public int ListId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ShowCount { get; set; }
    }
}

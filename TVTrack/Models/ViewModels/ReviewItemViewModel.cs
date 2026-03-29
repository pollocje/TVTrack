namespace TVTrack.Models.ViewModels
{
    public class ReviewItemViewModel
    {
        public string ShowTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
    }
}

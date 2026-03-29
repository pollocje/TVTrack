namespace TVTrack.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<ShowCardViewModel> Results { get; set; } = new List<ShowCardViewModel>();
        public int TotalResults { get; set; }
        public int Page { get; set; }
    }
}

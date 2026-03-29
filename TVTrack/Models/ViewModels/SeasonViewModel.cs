namespace TVTrack.Models.ViewModels
{
    public class EpisodeViewModel
    {
        public int EpisodeNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Overview { get; set; }
        public string? StillUrl { get; set; }
        public string? AirDate { get; set; }
    }

    public class SeasonViewModel
    {
        public int SeasonNumber { get; set; }
        public string? Name { get; set; }
        public List<EpisodeViewModel> Episodes { get; set; } = new();
    }
}

namespace TVTrack.Models
{
    public class WatchList
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ShowId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public TVShow Show { get; set; } = null!;
    }
}

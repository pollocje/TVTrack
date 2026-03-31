namespace TVTrack.Models
{
    public class Rating
    {
        // Class for the rating object of a show, includes user, score, timestamp
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ShowId { get; set; }
        public int Score { get; set; } // 1–5
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public TVShow Show { get; set; } = null!;
    }
}

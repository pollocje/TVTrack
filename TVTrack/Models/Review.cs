namespace TVTrack.Models
{
    public class Review
    {
        // Class for the review object including user, show, comments, etc
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ShowId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public TVShow Show { get; set; } = null!;
    }
}

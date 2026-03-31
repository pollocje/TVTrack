namespace TVTrack.Models
{
    public class Follow
    {
        // Class representing a "follow" between two users, 
        public int Id { get; set; }
        public string FollowerId { get; set; } = string.Empty;
        public string FollowedId { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}

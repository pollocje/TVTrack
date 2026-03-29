namespace TVTrack.Models
{
    public class Follow
    {
        public int Id { get; set; }
        public string FollowerId { get; set; } = string.Empty;
        public string FollowedId { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}

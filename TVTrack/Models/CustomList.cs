namespace TVTrack.Models
{
    public class CustomList
    {
        // Class that represents user-created list of shows (eg. Sci Fi Faves, Dramas, etc)
        public int Id { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? ShareToken { get; set; }

        public AppUser Owner { get; set; } = null!;
        public ICollection<CustomListItem> Items { get; set; } = new List<CustomListItem>();
    }
}

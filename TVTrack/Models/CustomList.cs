namespace TVTrack.Models
{
    public class CustomList
    {
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

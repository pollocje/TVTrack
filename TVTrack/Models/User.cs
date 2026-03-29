using Microsoft.AspNetCore.Identity;

namespace TVTrack.Models
{
    public class AppUser : IdentityUser
    {
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<CustomList> CustomLists { get; set; } = new List<CustomList>();
        public ICollection<WatchList> WatchList { get; set; } = new List<WatchList>();
    }
}

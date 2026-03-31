using Microsoft.AspNetCore.Identity;

namespace TVTrack.Models
{
    public class AppUser : IdentityUser
    {
        // class for user, includes bio, and relations to lows, lists, watchlist
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public ICollection<ShowLog> ShowLogs { get; set; } = new List<ShowLog>();
        public ICollection<CustomList> CustomLists { get; set; } = new List<CustomList>();
        public ICollection<WatchList> WatchList { get; set; } = new List<WatchList>();
    }
}

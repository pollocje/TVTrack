using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TVTrack.Models;

namespace TVTrack.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Main tables used by the app.
        public DbSet<TVShow> Shows { get; set; }
        public DbSet<ShowLog> ShowLogs { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<WatchList> WatchList { get; set; }
        public DbSet<CustomList> CustomLists { get; set; }
        public DbSet<CustomListItem> CustomListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }

    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Needed so EF tools can create migrations from the command line.
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TVTrackDb;Trusted_Connection=True;MultipleActiveResultSets=true");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

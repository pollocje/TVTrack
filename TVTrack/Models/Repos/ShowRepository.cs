using Microsoft.EntityFrameworkCore;
using TVTrack.Data;

namespace TVTrack.Models.Repos
{
    public class ShowRepository
    {
        private readonly AppDbContext _db;

        public ShowRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<TVShow?> GetByTmdbIdAsync(int tmdbId)
        {
            return await _db.Shows.FirstOrDefaultAsync(s => s.TmdbId == tmdbId);
        }

        public async Task<TVShow> CreateAsync(TVShow show)
        {
            _db.Shows.Add(show);
            await _db.SaveChangesAsync();
            return show;
        }

        // ── Watchlist ──────────────────────────────────────

        public async Task<bool> IsInWatchlistAsync(string userId, int showId)
        {
            return await _db.WatchList.AnyAsync(w => w.UserId == userId && w.ShowId == showId);
        }

        public async Task AddToWatchlistAsync(string userId, int showId)
        {
            if (!await IsInWatchlistAsync(userId, showId))
            {
                _db.WatchList.Add(new WatchList { UserId = userId, ShowId = showId });
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWatchlistAsync(string userId, int showId)
        {
            var entry = await _db.WatchList.FirstOrDefaultAsync(w => w.UserId == userId && w.ShowId == showId);
            if (entry != null)
            {
                _db.WatchList.Remove(entry);
                await _db.SaveChangesAsync();
            }
        }

        // ── Show Logs ──────────────────────────────────────

        public async Task AddLogAsync(ShowLog log)
        {
            _db.ShowLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<List<ShowLog>> GetShowLogsAsync(int showId)
        {
            return await _db.ShowLogs
                .Include(l => l.User)
                .Where(l => l.ShowId == showId)
                .OrderByDescending(l => l.WatchedOn)
                .ToListAsync();
        }

        public async Task<List<ShowLog>> GetUserLogsAsync(string userId)
        {
            return await _db.ShowLogs
                .Where(l => l.UserId == userId)
                .Include(l => l.Show)
                .OrderByDescending(l => l.WatchedOn)
                .ToListAsync();
        }

        public async Task<double?> GetAvgRatingAsync(int showId)
        {
            var hasRatings = await _db.ShowLogs.AnyAsync(l => l.ShowId == showId && l.Rating != null);
            if (!hasRatings) return null;
            return await _db.ShowLogs
                .Where(l => l.ShowId == showId && l.Rating != null)
                .AverageAsync(l => (double)l.Rating!.Value);
        }

        // ── Profile data ───────────────────────────────────

        public async Task<List<TVShow>> GetUserWatchlistAsync(string userId)
        {
            return await _db.WatchList
                .Where(w => w.UserId == userId)
                .Select(w => w.Show)
                .ToListAsync();
        }

        // ── Custom Lists ────────────────────────────────────

        public async Task<List<CustomList>> GetUserListsAsync(string userId)
        {
            return await _db.CustomLists
                .Include(l => l.Items)
                .Where(l => l.OwnerId == userId)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<CustomList> CreateListAsync(string userId, string name)
        {
            var list = new CustomList { OwnerId = userId, Name = name };
            _db.CustomLists.Add(list);
            await _db.SaveChangesAsync();
            return list;
        }

        public async Task AddShowToListAsync(int listId, string userId, int showId)
        {
            var list = await _db.CustomLists.FirstOrDefaultAsync(l => l.Id == listId && l.OwnerId == userId);
            if (list == null) return;

            bool alreadyAdded = await _db.CustomListItems
                .AnyAsync(i => i.CustomListId == listId && i.ShowId == showId);

            if (!alreadyAdded)
            {
                _db.CustomListItems.Add(new CustomListItem { CustomListId = listId, ShowId = showId });
                await _db.SaveChangesAsync();
            }
        }
    }
}

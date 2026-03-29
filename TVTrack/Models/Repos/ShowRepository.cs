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

        // ── Ratings ────────────────────────────────────────

        public async Task<int?> GetUserRatingAsync(string userId, int showId)
        {
            var rating = await _db.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.ShowId == showId);
            return rating?.Score;
        }

        public async Task SetRatingAsync(string userId, int showId, int score)
        {
            var existing = await _db.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.ShowId == showId);
            if (existing != null)
            {
                existing.Score = score;
                existing.RatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Ratings.Add(new Rating { UserId = userId, ShowId = showId, Score = score });
            }
            await _db.SaveChangesAsync();
        }

        public async Task<double?> GetAvgRatingAsync(int showId)
        {
            var hasRatings = await _db.Ratings.AnyAsync(r => r.ShowId == showId);
            if (!hasRatings) return null;
            return await _db.Ratings.Where(r => r.ShowId == showId).AverageAsync(r => (double)r.Score);
        }

        // ── Reviews ────────────────────────────────────────

        public async Task<List<Review>> GetReviewsAsync(int showId)
        {
            return await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.ShowId == showId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddReviewAsync(string userId, int showId, string comment)
        {
            _db.Reviews.Add(new Review { UserId = userId, ShowId = showId, Comment = comment });
            await _db.SaveChangesAsync();
        }

        // ── Profile data ───────────────────────────────────

        public async Task<List<TVShow>> GetUserWatchlistAsync(string userId)
        {
            return await _db.WatchList
                .Where(w => w.UserId == userId)
                .Select(w => w.Show)
                .ToListAsync();
        }

        public async Task<List<(TVShow Show, int Score)>> GetUserRatingsAsync(string userId)
        {
            var ratings = await _db.Ratings
                .Where(r => r.UserId == userId)
                .Include(r => r.Show)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            return ratings.Select(r => (r.Show, r.Score)).ToList();
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
            // Verify ownership
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

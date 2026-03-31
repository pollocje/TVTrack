using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TVTrack.Data;

namespace TVTrack.Models.Repos
{
    public class ShowRepository
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ShowRepository(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                .Include(w => w.Show)
                .Select(w => w.Show)
                .ToListAsync();
        }

        // ── Custom Lists ────────────────────────────────────

        public async Task<CustomList?> GetListByIdAsync(int listId)
        {
            return await _db.CustomLists
                .Include(l => l.Owner)
                .Include(l => l.Items)
                    .ThenInclude(i => i.Show)
                .FirstOrDefaultAsync(l => l.Id == listId);
        }

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

        // ── Social / Follow ────────────────────────────────

        public async Task<List<AppUser>> SearchUsersAsync(string query)
        {
            return await _userManager.Users
                .Where(u => u.UserName != null && u.UserName.Contains(query))
                .OrderBy(u => u.UserName)
                .Take(20)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
        {
            return await _db.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task FollowAsync(string followerId, string followedId)
        {
            if (followerId == followedId) return;
            if (await IsFollowingAsync(followerId, followedId)) return;

            _db.Follows.Add(new Follow { FollowerId = followerId, FollowedId = followedId });
            await _db.SaveChangesAsync();
        }

        public async Task UnfollowAsync(string followerId, string followedId)
        {
            var follow = await _db.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
            if (follow != null)
            {
                _db.Follows.Remove(follow);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<int> GetFollowersCountAsync(string userId)
        {
            return await _db.Follows.CountAsync(f => f.FollowedId == userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _db.Follows.CountAsync(f => f.FollowerId == userId);
        }

        public async Task<List<AppUser>> GetFollowersAsync(string userId)
        {
            var followerIds = await _db.Follows
                .Where(f => f.FollowedId == userId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            return await _userManager.Users
                .Where(u => followerIds.Contains(u.Id))
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }

        public async Task<List<AppUser>> GetFollowingAsync(string userId)
        {
            var followedIds = await _db.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowedId)
                .ToListAsync();

            return await _userManager.Users
                .Where(u => followedIds.Contains(u.Id))
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
    }
}

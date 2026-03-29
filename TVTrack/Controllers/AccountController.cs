using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVTrack.Models;
using TVTrack.Models.Repos;
using TVTrack.Models.ViewModels;

namespace TVTrack.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ShowRepository _showRepo;

        public AccountController(UserManager<AppUser> userManager, ShowRepository showRepo)
        {
            _userManager = userManager;
            _showRepo = showRepo;
        }

        // GET /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            const string posterBase = "https://image.tmdb.org/t/p/w185";

            var watchlist = await _showRepo.GetUserWatchlistAsync(user.Id);
            var lists = await _showRepo.GetUserListsAsync(user.Id);
            var ratings = await _showRepo.GetUserRatingsAsync(user.Id);

            var viewModel = new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.UserName ?? "Unknown",
                Bio = user.Bio,
                AvatarUrl = user.ProfilePictureUrl,
                Watchlist = watchlist.Select(s => new ShowCardViewModel
                {
                    TmdbId = s.TmdbId,
                    Title = s.Title,
                    PosterUrl = s.PosterPath != null ? posterBase + s.PosterPath : null
                }).ToList(),
                Lists = lists.Select(l => new ListSummaryViewModel
                {
                    ListId = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    ShowCount = l.Items.Count
                }).ToList(),
                Ratings = ratings.Select(r => new RatedShowViewModel
                {
                    TmdbId = r.Item1.TmdbId,
                    Title = r.Item1.Title,
                    PosterUrl = r.Item1.PosterPath != null ? posterBase + r.Item1.PosterPath : null,
                    Score = r.Item2
                }).ToList()
            };

            return View(viewModel);
        }
    }
}

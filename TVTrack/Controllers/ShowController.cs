using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVTrack.Models;
using TVTrack.Models.Repos;
using TVTrack.Models.ViewModels;
using static TVTrack.Models.ViewModels.ShowViewModel;
using TVTrack.Services;

namespace TVTrack.Controllers
{
    public class ShowController : Controller
    {
        private readonly ITmdbService _tmdbService;
        private readonly ShowRepository _showRepo;
        private readonly UserManager<AppUser> _userManager;

        public ShowController(ITmdbService tmdbService, ShowRepository showRepo, UserManager<AppUser> userManager)
        {
            _tmdbService = tmdbService;
            _showRepo = showRepo;
            _userManager = userManager;
        }

        // GET /Show/Search?query=...&page=1
        public async Task<IActionResult> Search(string? query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new SearchViewModel());

            var results = await _tmdbService.SearchShowsAsync(query, page);
            return View(results);
        }

        // GET /Show/Details/12345
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _tmdbService.GetShowDetailsAsync(id);
            if (viewModel == null)
                return NotFound();

            var dbShow = await _showRepo.GetByTmdbIdAsync(id);
            if (dbShow != null)
            {
                viewModel.AvgRating = await _showRepo.GetAvgRatingAsync(dbShow.Id);

                var reviews = await _showRepo.GetReviewsAsync(dbShow.Id);
                viewModel.Reviews = reviews.Select(r => new ReviewItemViewModel
                {
                    ShowTitle = viewModel.Title,
                    Rating = 0,
                    Comment = r.Comment,
                    PostedBy = r.User.UserName ?? "Unknown"
                }).ToList();

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User)!;
                    viewModel.IsInWatchlist = await _showRepo.IsInWatchlistAsync(userId, dbShow.Id);
                    viewModel.UserRating = await _showRepo.GetUserRatingAsync(userId, dbShow.Id);
                    var lists = await _showRepo.GetUserListsAsync(userId);
                    viewModel.UserLists = lists.Select(l => new CustomListSummary { Id = l.Id, Name = l.Name }).ToList();
                }
            }

            return View(viewModel);
        }

        // POST /Show/AddToWatchlist
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToWatchlist(int tmdbId)
        {
            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.AddToWatchlistAsync(userId, show.Id);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/RemoveFromWatchlist
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveFromWatchlist(int tmdbId)
        {
            var show = await _showRepo.GetByTmdbIdAsync(tmdbId);
            if (show != null)
            {
                var userId = _userManager.GetUserId(User)!;
                await _showRepo.RemoveFromWatchlistAsync(userId, show.Id);
            }
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/Rate
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Rate(int tmdbId, int score)
        {
            if (score < 1 || score > 5)
                return BadRequest();

            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.SetRatingAsync(userId, show.Id, score);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/Review
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Review(int tmdbId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return RedirectToAction(nameof(Details), new { id = tmdbId });

            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.AddReviewAsync(userId, show.Id, comment);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/AddToList
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToList(int tmdbId, int listId)
        {
            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.AddShowToListAsync(listId, userId, show.Id);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/CreateList
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateList(int tmdbId, string listName)
        {
            if (string.IsNullOrWhiteSpace(listName))
                return RedirectToAction(nameof(Details), new { id = tmdbId });

            var userId = _userManager.GetUserId(User)!;
            var list = await _showRepo.CreateListAsync(userId, listName.Trim());

            var show = await EnsureShowInDbAsync(tmdbId);
            if (show != null)
                await _showRepo.AddShowToListAsync(list.Id, userId, show.Id);

            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // Fetches show from TMDB and inserts into DB if it doesn't exist yet
        private async Task<TVShow?> EnsureShowInDbAsync(int tmdbId)
        {
            var existing = await _showRepo.GetByTmdbIdAsync(tmdbId);
            if (existing != null) return existing;

            var details = await _tmdbService.GetShowDetailsAsync(tmdbId);
            if (details == null) return null;

            return await _showRepo.CreateAsync(new TVShow
            {
                TmdbId = tmdbId,
                Title = details.Title,
                Overview = details.Overview,
                PosterPath = details.PosterPath,
                FirstAirDate = details.FirstAirDate,
                NumberOfSeasons = details.NumberOfSeasons
            });
        }
    }
}

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

                var logs = await _showRepo.GetShowLogsAsync(dbShow.Id);
                viewModel.RecentLogs = logs.Select(l => new ShowLogItemViewModel
                {
                    Username = l.User.UserName ?? "Unknown",
                    LogType = l.LogType,
                    SeasonNumber = l.SeasonNumber,
                    EpisodeNumber = l.EpisodeNumber,
                    EpisodeTitle = l.EpisodeTitle,
                    Rating = l.Rating,
                    Review = l.Review,
                    WatchedOn = l.WatchedOn
                }).ToList();

                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _userManager.GetUserId(User)!;
                    viewModel.IsInWatchlist = await _showRepo.IsInWatchlistAsync(userId, dbShow.Id);
                    var lists = await _showRepo.GetUserListsAsync(userId);
                    viewModel.UserLists = lists.Select(l => new CustomListSummary { Id = l.Id, Name = l.Name }).ToList();
                }
            }

            return View(viewModel);
        }

        // GET /Show/Season/12345/1
        [HttpGet("Show/Season/{id}/{season}")]
        public async Task<IActionResult> Season(int id, int season)
        {
            var viewModel = await _tmdbService.GetSeasonAsync(id, season);
            if (viewModel == null)
                return NotFound();

            return Json(viewModel);
        }

        // POST /Show/AddToWatchlist
        [HttpPost, Authorize]
        public async Task<IActionResult> AddToWatchlist(int tmdbId)
        {
            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.AddToWatchlistAsync(userId, show.Id);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/RemoveFromWatchlist
        [HttpPost, Authorize]
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

        // POST /Show/Log
        [HttpPost, Authorize]
        public async Task<IActionResult> Log(LogViewModel model)
        {
            var show = await EnsureShowInDbAsync(model.TmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;

            var log = new ShowLog
            {
                UserId = userId,
                ShowId = show.Id,
                LogType = model.LogType,
                SeasonNumber = model.LogType != LogType.Series ? model.SeasonNumber : null,
                EpisodeNumber = model.LogType == LogType.Episode ? model.EpisodeNumber : null,
                EpisodeTitle = model.LogType == LogType.Episode ? model.EpisodeTitle : null,
                Rating = model.Rating is >= 1 and <= 5 ? model.Rating : null,
                Review = string.IsNullOrWhiteSpace(model.Review) ? null : model.Review.Trim(),
                WatchedOn = model.WatchedOn == default ? DateTime.UtcNow : model.WatchedOn.ToUniversalTime()
            };

            await _showRepo.AddLogAsync(log);
            return RedirectToAction(nameof(Details), new { id = model.TmdbId });
        }

        // POST /Show/AddToList
        [HttpPost, Authorize]
        public async Task<IActionResult> AddToList(int tmdbId, int listId)
        {
            var show = await EnsureShowInDbAsync(tmdbId);
            if (show == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            await _showRepo.AddShowToListAsync(listId, userId, show.Id);
            return RedirectToAction(nameof(Details), new { id = tmdbId });
        }

        // POST /Show/CreateList
        [HttpPost, Authorize]
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

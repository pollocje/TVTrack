using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVTrack.Models;
using TVTrack.Models.Repos;
using TVTrack.Models.ViewModels;

namespace TVTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ShowRepository _showRepo;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ShowRepository showRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _showRepo = showRepo;
        }

        // GET /Account/Register
        [AllowAnonymous]
        public IActionResult Register() => View();

        // POST /Account/Register
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new AppUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET /Account/Login
        [AllowAnonymous]
        public IActionResult Login() => View();

        // POST /Account/Login
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            // Support login with either username or email
            var user = model.EmailOrUsername.Contains('@')
                ? await _userManager.FindByEmailAsync(model.EmailOrUsername)
                : await _userManager.FindByNameAsync(model.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);

            ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            return View(model);
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            const string posterBase = "https://image.tmdb.org/t/p/w185";

            var watchlist = await _showRepo.GetUserWatchlistAsync(user.Id);
            var lists = await _showRepo.GetUserListsAsync(user.Id);
            var logs = await _showRepo.GetUserLogsAsync(user.Id);

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
                RecentLogs = logs.Select(l => new LogEntryViewModel
                {
                    TmdbId = l.Show.TmdbId,
                    Title = l.Show.Title,
                    PosterUrl = l.Show.PosterPath != null ? posterBase + l.Show.PosterPath : null,
                    LogType = l.LogType,
                    SeasonNumber = l.SeasonNumber,
                    EpisodeNumber = l.EpisodeNumber,
                    EpisodeTitle = l.EpisodeTitle,
                    Rating = l.Rating,
                    ReviewText = l.Review,
                    LoggedAt = l.WatchedOn
                }).ToList()
            };

            return View(viewModel);
        }
    }
}

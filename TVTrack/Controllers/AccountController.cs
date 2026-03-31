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
        private readonly IWebHostEnvironment _env;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ShowRepository showRepo, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _showRepo = showRepo;
            _env = env;
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

            var viewModel = await BuildProfileViewModelAsync(user, _userManager.GetUserId(User));
            return View(viewModel);
        }

        // GET /Account/Profile/{username}
        public async Task<IActionResult> ViewProfile(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var viewModel = await BuildProfileViewModelAsync(user, currentUserId);
            return View("Profile", viewModel);
        }

        // POST /Account/Follow
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(string username)
        {
            var target = await _userManager.FindByNameAsync(username);
            if (target == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User)!;
            await _showRepo.FollowAsync(currentUserId, target.Id);
            return RedirectToAction(nameof(ViewProfile), new { username });
        }

        // POST /Account/Unfollow
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(string username)
        {
            var target = await _userManager.FindByNameAsync(username);
            if (target == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User)!;
            await _showRepo.UnfollowAsync(currentUserId, target.Id);
            return RedirectToAction(nameof(ViewProfile), new { username });
        }

        // GET /Account/Search?query=
        public async Task<IActionResult> Search(string? query)
        {
            var viewModel = new UserSearchViewModel { Query = query ?? string.Empty };

            if (!string.IsNullOrWhiteSpace(query))
            {
                var users = await _showRepo.SearchUsersAsync(query.Trim());
                viewModel.Results = users.Select(u => new UserCardViewModel
                {
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    AvatarUrl = u.ProfilePictureUrl
                }).ToList();
            }

            return View(viewModel);
        }

        // GET /Account/Followers/{username}
        public async Task<IActionResult> Followers(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            var users = await _showRepo.GetFollowersAsync(user.Id);
            return View("FollowList", new FollowListViewModel
            {
                Username = username,
                ListType = "Followers",
                Users = users.Select(u => new UserCardViewModel
                {
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    AvatarUrl = u.ProfilePictureUrl
                }).ToList()
            });
        }

        // GET /Account/Following/{username}
        public async Task<IActionResult> Following(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound();

            var users = await _showRepo.GetFollowingAsync(user.Id);
            return View("FollowList", new FollowListViewModel
            {
                Username = username,
                ListType = "Following",
                Users = users.Select(u => new UserCardViewModel
                {
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    AvatarUrl = u.ProfilePictureUrl
                }).ToList()
            });
        }

        // GET /Account/EditProfile
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(new EditProfileViewModel
            {
                Bio = user.Bio,
                CurrentAvatarUrl = user.ProfilePictureUrl
            });
        }

        // POST /Account/EditProfile
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();

            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(model.Avatar.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Avatar", "Only image files are allowed (jpg, png, gif, webp).");
                    model.CurrentAvatarUrl = user.ProfilePictureUrl;
                    return View(model);
                }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{user.Id}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.Avatar.CopyToAsync(stream);

                user.ProfilePictureUrl = $"/uploads/avatars/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Profile));
        }

        private async Task<ProfileViewModel> BuildProfileViewModelAsync(AppUser user, string? currentUserId)
        {
            const string posterBase = "https://image.tmdb.org/t/p/w185";

            var watchlist = await _showRepo.GetUserWatchlistAsync(user.Id);
            var lists = await _showRepo.GetUserListsAsync(user.Id);
            var logs = await _showRepo.GetUserLogsAsync(user.Id);
            var followersCount = await _showRepo.GetFollowersCountAsync(user.Id);
            var followingCount = await _showRepo.GetFollowingCountAsync(user.Id);
            var isFollowing = currentUserId != null && currentUserId != user.Id
                && await _showRepo.IsFollowingAsync(currentUserId, user.Id);

            return new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.UserName ?? "Unknown",
                Bio = user.Bio,
                AvatarUrl = user.ProfilePictureUrl,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
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
        }
    }
}

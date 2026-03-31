using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVTrack.Models;
using TVTrack.Models.Repos;
using TVTrack.Models.ViewModels;

namespace TVTrack.Controllers
{
    public class ListController : Controller
    {
        private readonly ShowRepository _showRepo;
        private readonly UserManager<AppUser> _userManager;

        public ListController(ShowRepository showRepo, UserManager<AppUser> userManager)
        {
            _showRepo = showRepo;
            _userManager = userManager;
        }

        // GET /List/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var list = await _showRepo.GetListByIdAsync(id);
            if (list == null) return NotFound();

            const string posterBase = "https://image.tmdb.org/t/p/w185";
            var currentUserId = _userManager.GetUserId(User);

            var viewModel = new ListDetailViewModel
            {
                ListId = list.Id,
                Name = list.Name,
                Description = list.Description,
                OwnerUsername = list.Owner.UserName ?? "Unknown",
                IsOwner = currentUserId == list.OwnerId,
                Shows = list.Items.Select(i => new ShowCardViewModel
                {
                    TmdbId = i.Show.TmdbId,
                    Title = i.Show.Title,
                    PosterUrl = i.Show.PosterPath != null ? posterBase + i.Show.PosterPath : null
                }).ToList()
            };

            return View(viewModel);
        }
    }
}

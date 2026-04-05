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

        // Loads a list by its database ID and displays it.
        // GET /List/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var list = await _showRepo.GetListByIdAsync(id);
            if (list == null) return NotFound();

            return View(BuildViewModel(list));
        }

        // Public route that lets anyone view a list using its share token instead of
        // its ID. Reuses the same Details view so the page looks identical.
        // GET /List/Share/{token}
        public async Task<IActionResult> Share(string token)
        {
            var list = await _showRepo.GetListByShareTokenAsync(token);
            if (list == null) return NotFound();

            return View("Details", BuildViewModel(list));
        }

        // Deletes the list and all its entries, then sends the user back to their
        // profile. The repo checks ownership so only the list owner can delete it.
        // POST /List/Delete
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int listId)
        {
            var userId = _userManager.GetUserId(User)!;
            await _showRepo.DeleteListAsync(listId, userId);
            return RedirectToAction("Profile", "Account");
        }

        // Removes one show from a list. The view passes the TMDB ID since that's what
        // show cards store, so we look up the DB record first before calling the repo.
        // POST /List/RemoveShow
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveShow(int listId, int tmdbId)
        {
            var userId = _userManager.GetUserId(User)!;
            var show = await _showRepo.GetByTmdbIdAsync(tmdbId);
            if (show != null)
                await _showRepo.RemoveShowFromListAsync(listId, userId, show.Id);

            return RedirectToAction(nameof(Details), new { id = listId });
        }

        // Builds the view model from a CustomList domain object. Kept as a private
        // helper so both Details and Share can use it without repeating the mapping.
        private ListDetailViewModel BuildViewModel(TVTrack.Models.CustomList list)
        {
            const string posterBase = "https://image.tmdb.org/t/p/w185";
            var currentUserId = _userManager.GetUserId(User);

            return new ListDetailViewModel
            {
                ListId = list.Id,
                Name = list.Name,
                Description = list.Description,
                OwnerUsername = list.Owner.UserName ?? "Unknown",
                IsOwner = currentUserId == list.OwnerId,
                ShareToken = list.ShareToken,
                Shows = list.Items.Select(i => new ShowCardViewModel
                {
                    TmdbId = i.Show.TmdbId,
                    Title = i.Show.Title,
                    PosterUrl = i.Show.PosterPath != null ? posterBase + i.Show.PosterPath : null
                }).ToList()
            };
        }
    }
}

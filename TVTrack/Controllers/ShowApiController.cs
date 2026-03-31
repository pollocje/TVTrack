// Author: Jeffrey Pollock
// Web API controller that exposes show search and details as JSON endpoints.

using Microsoft.AspNetCore.Mvc;
using TVTrack.Services;

namespace TVTrack.Controllers
{
    [ApiController]
    [Route("api/shows")]
    public class ShowApiController : ControllerBase
    {
        private readonly ITmdbService _tmdbService;

        public ShowApiController(ITmdbService tmdbService)
        {
            _tmdbService = tmdbService;
        }

        // Search for TV shows by title.
        // GET /api/shows/search?query=breaking+bad&page=1
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("query is required.");

            var results = await _tmdbService.SearchShowsAsync(query, page);
            return Ok(results);
        }

        // Get details for a specific TV show by TMDB ID.
        // GET /api/shows/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var show = await _tmdbService.GetShowDetailsAsync(id);
            if (show == null)
                return NotFound();

            return Ok(show);
        }

        // Get episode details for a specific season of a show.
        // GET /api/shows/{id}/season/{season}
        [HttpGet("{id}/season/{season}")]
        public async Task<IActionResult> Season(int id, int season)
        {
            var result = await _tmdbService.GetSeasonAsync(id, season);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}

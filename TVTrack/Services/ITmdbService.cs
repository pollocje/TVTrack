using TVTrack.Models.ViewModels;

namespace TVTrack.Services
{
    public interface ITmdbService
    {
        Task<SearchViewModel> SearchShowsAsync(string query, int page = 1);
        Task<ShowViewModel?> GetShowDetailsAsync(int tmdbId);
        Task<SeasonViewModel?> GetSeasonAsync(int tmdbId, int seasonNumber);
    }
}

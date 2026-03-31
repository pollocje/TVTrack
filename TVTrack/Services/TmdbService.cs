using System.Text.Json;
using System.Text.Json.Serialization;
using TVTrack.Models.ViewModels;

namespace TVTrack.Services
{
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _imageBaseUrl;

        public TmdbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Tmdb:ApiKey"]!;
            _imageBaseUrl = configuration["Tmdb:ImageBaseUrl"]!;
            _httpClient.BaseAddress = new Uri(configuration["Tmdb:BaseUrl"]!);
        }

        public async Task<SearchViewModel> SearchShowsAsync(string query, int page = 1)
        {
            var url = $"/3/search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&page={page}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResponse>(json);

            // Convert TMDB's response shape into the simpler view model
            // used by our MVC page and API controller.
            return new SearchViewModel
            {
                Query = query,
                Page = result?.Page ?? 1,
                TotalResults = result?.TotalResults ?? 0,
                Results = result?.Results.Select(r => new ShowCardViewModel
                {
                    TmdbId = r.Id,
                    Title = r.Name,
                    PosterUrl = r.PosterPath != null ? $"{_imageBaseUrl}{r.PosterPath}" : null
                }).ToList() ?? new List<ShowCardViewModel>()
            };
        }

        public async Task<ShowViewModel?> GetShowDetailsAsync(int tmdbId)
        {
            var url = $"/3/tv/{tmdbId}?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbShowDetails>(json);

            if (result == null)
                return null;

            return new ShowViewModel
            {
                TmdbId = result.Id,
                Title = result.Name,
                Overview = result.Overview,
                PosterPath = result.PosterPath,
                PosterUrl = result.PosterPath != null ? $"{_imageBaseUrl}{result.PosterPath}" : null,
                FirstAirDate = result.FirstAirDate,
                NumberOfSeasons = result.NumberOfSeasons
            };
        }

        public async Task<SeasonViewModel?> GetSeasonAsync(int tmdbId, int seasonNumber)
        {
            var url = $"/3/tv/{tmdbId}/season/{seasonNumber}?api_key={_apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSeasonDetails>(json);

            if (result == null)
                return null;

            // Season data is only pulled when needed so we do not
            // store every episode in the local database.
            return new SeasonViewModel
            {
                SeasonNumber = result.SeasonNumber,
                Name = result.Name,
                Episodes = result.Episodes.Select(e => new EpisodeViewModel
                {
                    EpisodeNumber = e.EpisodeNumber,
                    Name = e.Name,
                    Overview = e.Overview,
                    StillUrl = e.StillPath != null ? $"{_imageBaseUrl}{e.StillPath}" : null,
                    AirDate = e.AirDate
                }).ToList()
            };
        }

        // --- Private DTOs for deserializing TMDB responses ---

        private class TmdbSearchResponse
        {
            [JsonPropertyName("page")]
            public int Page { get; set; }

            [JsonPropertyName("results")]
            public List<TmdbShowResult> Results { get; set; } = new();

            [JsonPropertyName("total_results")]
            public int TotalResults { get; set; }
        }

        private class TmdbShowResult
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("overview")]
            public string? Overview { get; set; }

            [JsonPropertyName("poster_path")]
            public string? PosterPath { get; set; }

            [JsonPropertyName("first_air_date")]
            public string? FirstAirDate { get; set; }
        }

        private class TmdbShowDetails : TmdbShowResult
        {
            [JsonPropertyName("number_of_seasons")]
            public int? NumberOfSeasons { get; set; }
        }

        private class TmdbSeasonDetails
        {
            [JsonPropertyName("season_number")]
            public int SeasonNumber { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("episodes")]
            public List<TmdbEpisode> Episodes { get; set; } = new();
        }

        private class TmdbEpisode
        {
            [JsonPropertyName("episode_number")]
            public int EpisodeNumber { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("overview")]
            public string? Overview { get; set; }

            [JsonPropertyName("still_path")]
            public string? StillPath { get; set; }

            [JsonPropertyName("air_date")]
            public string? AirDate { get; set; }
        }
    }
}

using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TVTrack.Controllers;
using TVTrack.Models.ViewModels;
using TVTrack.Services;

namespace TVTrack.Tests;

// Made by Muhammed Cengiz.
// These tests cover the TMDB service mapping and a few basic API controller cases.
public class TmdbAndApiTests
{
    [Test]
    public async Task SearchShowsAsync_MapsSearchResultsIntoViewModel()
    {
        // Fake the TMDB response so this test only checks our mapping code.
        var service = CreateTmdbService(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/3/search/tv")
            {
                return JsonResponse("""
                {
                  "page": 2,
                  "total_results": 1,
                  "results": [
                    {
                      "id": 1399,
                      "name": "Game of Thrones",
                      "poster_path": "/got.jpg"
                    }
                  ]
                }
                """);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.SearchShowsAsync("game of thrones", 2);

        Assert.That(result.Query, Is.EqualTo("game of thrones"));
        Assert.That(result.Page, Is.EqualTo(2));
        Assert.That(result.TotalResults, Is.EqualTo(1));
        Assert.That(result.Results, Has.Count.EqualTo(1));
        Assert.That(result.Results[0].TmdbId, Is.EqualTo(1399));
        Assert.That(result.Results[0].Title, Is.EqualTo("Game of Thrones"));
        Assert.That(result.Results[0].PosterUrl, Is.EqualTo("https://image.tmdb.org/t/p/w500/got.jpg"));
    }

    [Test]
    public async Task GetSeasonAsync_MapsEpisodeDetails()
    {
        // Same idea here: keep it isolated and only test the season mapping.
        var service = CreateTmdbService(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/3/tv/1399/season/1")
            {
                return JsonResponse("""
                {
                  "season_number": 1,
                  "name": "Season 1",
                  "episodes": [
                    {
                      "episode_number": 1,
                      "name": "Winter Is Coming",
                      "overview": "Ned visits Winterfell.",
                      "still_path": "/winter.jpg",
                      "air_date": "2011-04-17"
                    }
                  ]
                }
                """);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await service.GetSeasonAsync(1399, 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SeasonNumber, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Season 1"));
        Assert.That(result.Episodes, Has.Count.EqualTo(1));
        Assert.That(result.Episodes[0].Name, Is.EqualTo("Winter Is Coming"));
        Assert.That(result.Episodes[0].StillUrl, Is.EqualTo("https://image.tmdb.org/t/p/w500/winter.jpg"));
    }

    [Test]
    public async Task ShowApiSearch_ReturnsBadRequest_WhenQueryIsBlank()
    {
        var controller = new ShowApiController(new FakeTmdbService());

        var result = await controller.Search("", 1);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ShowApiDetails_ReturnsNotFound_WhenShowDoesNotExist()
    {
        var controller = new ShowApiController(new FakeTmdbService());

        var result = await controller.Details(999999);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    private static TmdbService CreateTmdbService(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tmdb:ApiKey"] = "test-key",
                ["Tmdb:BaseUrl"] = "https://api.themoviedb.org",
                ["Tmdb:ImageBaseUrl"] = "https://image.tmdb.org/t/p/w500"
            })
            .Build();

        var client = new HttpClient(new FakeHttpMessageHandler(responseFactory))
        {
            BaseAddress = new Uri("https://api.themoviedb.org")
        };

        // Build the real service with fake HTTP responses so the tests stay fast.
        return new TmdbService(client, config);
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory(request));
        }
    }

    private sealed class FakeTmdbService : ITmdbService
    {
        public Task<SearchViewModel> SearchShowsAsync(string query, int page = 1)
        {
            return Task.FromResult(new SearchViewModel());
        }

        public Task<ShowViewModel?> GetShowDetailsAsync(int tmdbId)
        {
            return Task.FromResult<ShowViewModel?>(null);
        }

        public Task<SeasonViewModel?> GetSeasonAsync(int tmdbId, int seasonNumber)
        {
            return Task.FromResult<SeasonViewModel?>(null);
        }
    }
}

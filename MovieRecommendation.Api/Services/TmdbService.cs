using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using MovieRecommendation.Api.DTOs.ExternalRecommendations;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Services;

public class TmdbService : ITmdbService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    public TmdbService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public async Task<Dictionary<string, int>>
        GetMovieGenresAsync()
    {
        const string cacheKey =
            "tmdb_movie_genres";

        if (_cache.TryGetValue(
            cacheKey,
            out Dictionary<string, int>? cachedGenres))
        {
            return cachedGenres!;
        }

        var client =
            _httpClientFactory.CreateClient("TMDB");

        var response =
            await client.GetFromJsonAsync<TmdbGenreResponse>(
                "genre/movie/list?language=en");

        if (response == null)
        {
            return new Dictionary<string, int>();
        }

        var genres =
            response.Genres.ToDictionary(
                g => g.Name,
                g => g.Id,
                StringComparer.OrdinalIgnoreCase);

        _cache.Set(
            cacheKey,
            genres,
            TimeSpan.FromHours(24));

        return genres;
    }

    public async Task<List<TmdbMovieCandidateDto>>
        GetMovieCandidatesAsync(
            IEnumerable<int> genreIds)
    {
        var genres = genreIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var genreKey =
            genres.Any()
                ? string.Join("_", genres)
                : "discovery";

        var cacheKey =
            $"tmdb_candidates_{genreKey}";

        if (_cache.TryGetValue(
            cacheKey,
            out List<TmdbMovieCandidateDto>? cachedCandidates))
        {
            return cachedCandidates!;
        }

        var client =
            _httpClientFactory.CreateClient("TMDB");

        var genreQuery =
            string.Join("|", genres);

        var today =
            DateTime.UtcNow
                .ToString("yyyy-MM-dd");

        var allCandidates =
            new List<TmdbMovieCandidateDto>();

        for (var page = 1; page <= 3; page++)
        {
            var url =
                "discover/movie" +
                "?include_adult=false" +
                "&include_video=false" +
                "&language=en-US" +
                "&sort_by=popularity.desc" +
                "&vote_count.gte=200" +
                "&vote_average.gte=6.0" +
                $"&primary_release_date.lte={today}" +
                $"&page={page}";

            if (genres.Any())
            {
                url +=
                    $"&with_genres={Uri.EscapeDataString(genreQuery)}";
            }

            var response =
                await client
                    .GetFromJsonAsync<TmdbDiscoverResponse>(
                        url);

            if (response == null)
            {
                continue;
            }

            allCandidates.AddRange(
                response.Results);
        }

        var candidates =
            allCandidates
                .Where(movie =>
                    movie.VoteCount >= 200 &&
                    movie.VoteAverage >= 6.0)
                .GroupBy(movie =>
                    movie.Id)
                .Select(group =>
                    group.First())
                .ToList();

        _cache.Set(
            cacheKey,
            candidates,
            TimeSpan.FromMinutes(10));

        return candidates;
    }

    public async Task<int?>
        ResolveTmdbMovieIdAsync(
            string id,
            string source)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        id = id.Trim();
        source = source.Trim();

        // Already a TMDB ID
        if (source.Equals(
            "tmdb",
            StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(
                id,
                out var tmdbId))
            {
                return tmdbId;
            }

            return null;
        }

        // Currently we only normalize IMDb -> TMDB
        if (!source.Equals(
            "imdb",
            StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var cacheKey =
            $"tmdb_imdb_mapping_{id}";

        if (_cache.TryGetValue(
            cacheKey,
            out int cachedTmdbId))
        {
            return cachedTmdbId;
        }

        var client =
            _httpClientFactory.CreateClient("TMDB");

        var externalId =
            Uri.EscapeDataString(id);

        var response =
            await client
                .GetFromJsonAsync<TmdbFindResponse>(
                    $"find/{externalId}" +
                    "?external_source=imdb_id" +
                    "&language=en-US");

        var resolvedTmdbId =
            response?
                .MovieResults
                .FirstOrDefault()
                ?.Id;

        if (resolvedTmdbId.HasValue)
        {
            _cache.Set(
                cacheKey,
                resolvedTmdbId.Value,
                TimeSpan.FromHours(24));
        }

        return resolvedTmdbId;
    }

    private class TmdbGenreResponse
    {
        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } =
            new();
    }

    private class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } =
            string.Empty;
    }

    private class TmdbDiscoverResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMovieCandidateDto> Results { get; set; } =
            new();
    }

    private class TmdbFindResponse
    {
        [JsonPropertyName("movie_results")]
        public List<TmdbFindMovieResult> MovieResults { get; set; } =
            new();
    }

    private class TmdbFindMovieResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
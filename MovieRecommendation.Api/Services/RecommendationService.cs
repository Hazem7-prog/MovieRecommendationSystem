using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.AI;
using MovieRecommendation.Api.DTOs.Recommendations;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        ApplicationDbContext context,
        IAIService aiService,
        IMemoryCache cache,
        ILogger<RecommendationService> logger)
    {
        _context = context;
        _aiService = aiService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<RecommendationResponseDto>>
        GetRecommendationsAsync(string userId)
    {
        var totalStopwatch = Stopwatch.StartNew();

        var cacheKey = $"recommendations_{userId}";

        _logger.LogInformation(
            "Recommendation request started.");

        if (_cache.TryGetValue(
            cacheKey,
            out List<RecommendationResponseDto>? cachedRecommendations))
        {
            totalStopwatch.Stop();

            _logger.LogInformation(
                "Recommendation Cache HIT. Completed in {ElapsedMs} ms.",
                totalStopwatch.ElapsedMilliseconds);

            return cachedRecommendations!;
        }

        _logger.LogInformation(
            "Recommendation Cache MISS. Generating new recommendations.");

        var highRatedMovies = await _context.Ratings
            .Where(r =>
                r.UserId == userId &&
                r.Score >= 7)
            .Include(r => r.Movie)
                .ThenInclude(m => m.Genres)
            .ToListAsync();

        var favoriteMovies = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Movie)
                .ThenInclude(m => m.Genres)
            .Select(f => f.Movie)
            .ToListAsync();

        var watchlistMovies = await _context.Watchlists
            .Where(w => w.UserId == userId)
            .Include(w => w.Movie)
                .ThenInclude(m => m.Genres)
            .Select(w => w.Movie)
            .ToListAsync();

        _logger.LogInformation(
            "Loaded user preference data. HighRatings: {HighRatings}, Favorites: {Favorites}, Watchlist: {Watchlist}",
            highRatedMovies.Count,
            favoriteMovies.Count,
            watchlistMovies.Count);

        var hasUserPreferences =
            highRatedMovies.Any() ||
            favoriteMovies.Any() ||
            watchlistMovies.Any();

        // Cold Start
        if (!hasUserPreferences)
        {
            _logger.LogInformation(
                "Cold start detected. Using discovery recommendations.");

            var coldStartMovies = await _context.Movies
                .Where(m => !m.IsDeleted)
                .Include(m => m.Genres)
                .Include(m => m.Ratings)
                .Select(movie => new
                {
                    Movie = movie,

                    AverageRating = movie.Ratings.Any()
                        ? movie.Ratings.Average(r => r.Score)
                        : 0
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.Movie.Id)
                .Take(10)
                .ToListAsync();

            var coldStartRecommendations = coldStartMovies
                .Select(item => new RecommendationResponseDto
                {
                    MovieId = item.Movie.Id,

                    Title = item.Movie.Title,

                    PosterUrl = item.Movie.PosterUrl,

                    AverageRating = item.AverageRating,

                    Genres = item.Movie.Genres
                        .Select(g => g.Name)
                        .ToList(),

                    RecommendationScore = item.AverageRating,

                    RecommendationType = "Discovery",

                    Reason =
                        "A discovery pick while we learn your movie preferences.",

                    Confidence = 0
                })
                .ToList();

            var coldStartCacheOptions =
                new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(
                        TimeSpan.FromMinutes(5));

            _cache.Set(
                cacheKey,
                coldStartRecommendations,
                coldStartCacheOptions);

            totalStopwatch.Stop();

            _logger.LogInformation(
                "Cold start recommendations completed in {ElapsedMs} ms. RecommendationsCount: {Count}",
                totalStopwatch.ElapsedMilliseconds,
                coldStartRecommendations.Count);

            return coldStartRecommendations;
        }

        var genreScores =
            new Dictionary<int, double>();

        var genreNames =
            new Dictionary<int, string>();

        // High Rating = +3
        foreach (var rating in highRatedMovies)
        {
            foreach (var genre in rating.Movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                    genreNames[genre.Id] = genre.Name;
                }

                genreScores[genre.Id] += 3;
            }
        }

        // Favorite = +2
        foreach (var movie in favoriteMovies)
        {
            foreach (var genre in movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                    genreNames[genre.Id] = genre.Name;
                }

                genreScores[genre.Id] += 2;
            }
        }

        // Watchlist = +1
        foreach (var movie in watchlistMovies)
        {
            foreach (var genre in movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                    genreNames[genre.Id] = genre.Name;
                }

                genreScores[genre.Id] += 1;
            }
        }

        var userMovieIds = highRatedMovies
            .Select(r => r.MovieId)
            .Concat(favoriteMovies.Select(m => m.Id))
            .Concat(watchlistMovies.Select(m => m.Id))
            .Distinct()
            .ToList();

        var userInteractions =
            new List<string>();

        foreach (var rating in highRatedMovies)
        {
            userInteractions.Add(
                $"Rated {rating.Movie.Title} {rating.Score}/10");
        }

        foreach (var movie in favoriteMovies)
        {
            userInteractions.Add(
                $"Added {movie.Title} to favorites");
        }

        foreach (var movie in watchlistMovies)
        {
            userInteractions.Add(
                $"Added {movie.Title} to watchlist");
        }

        var preferredGenres = genreScores
            .OrderByDescending(g => g.Value)
            .Select(g => genreNames[g.Key])
            .Distinct()
            .ToList();

        var candidateMovies = await _context.Movies
            .Where(m =>
                !m.IsDeleted &&
                !userMovieIds.Contains(m.Id))
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .ToListAsync();

        var scoredMovies = candidateMovies
            .Select(movie =>
            {
                var genreScore = movie.Genres
                    .Where(g =>
                        genreScores.ContainsKey(g.Id))
                    .Sum(g =>
                        genreScores[g.Id]);

                var averageRating =
                    movie.Ratings.Any()
                        ? movie.Ratings.Average(r => r.Score)
                        : 0;

                var recommendationScore =
                    genreScore +
                    (averageRating * 0.2);

                return new
                {
                    Movie = movie,
                    GenreScore = genreScore,
                    AverageRating = averageRating,
                    RecommendationScore = recommendationScore
                };
            })
            .OrderByDescending(
                x => x.RecommendationScore)
            .Take(10)
            .ToList();

        _logger.LogInformation(
            "Recommendation engine produced {MoviesCount} movies.",
            scoredMovies.Count);

        if (!scoredMovies.Any())
        {
            totalStopwatch.Stop();

            return new List<RecommendationResponseDto>();
        }

        var moviesForAI = scoredMovies
            .Select(item =>
                new MovieForAIRequestDto
                {
                    MovieId = item.Movie.Id,

                    Title = item.Movie.Title,

                    Genres = item.Movie.Genres
                        .Select(g => g.Name)
                        .ToList()
                })
            .ToList();

        var aiExplanations =
            new List<AIRecommendationExplanationDto>();

        var aiSucceeded = false;

        var aiStopwatch =
            Stopwatch.StartNew();

        try
        {
            aiExplanations =
                await _aiService
                    .GenerateRecommendationExplanationsAsync(
                        string.Join(
                            ", ",
                            preferredGenres),

                        string.Join(
                            "; ",
                            userInteractions),

                        moviesForAI);

            aiSucceeded = true;

            aiStopwatch.Stop();

            _logger.LogInformation(
                "AI explanation succeeded in {ElapsedMs} ms.",
                aiStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            aiStopwatch.Stop();

            _logger.LogWarning(
                ex,
                "AI explanation failed after {ElapsedMs} ms. Backend fallback will be used.",
                aiStopwatch.ElapsedMilliseconds);
        }

        var recommendations = scoredMovies
            .Select(item =>
            {
                var aiResult = aiExplanations
                    .FirstOrDefault(a =>
                        a.MovieId == item.Movie.Id);

                var fallbackReason =
                    GenerateFallbackReason(
                        item.Movie.Genres
                            .Select(g => g.Name)
                            .ToList(),

                        preferredGenres);

                var recommendationType =
                    item.GenreScore > 0
                        ? "Personalized"
                        : "Discovery";

                return new RecommendationResponseDto
                {
                    MovieId = item.Movie.Id,

                    Title = item.Movie.Title,

                    PosterUrl =
                        item.Movie.PosterUrl,

                    AverageRating =
                        item.AverageRating,

                    Genres = item.Movie.Genres
                        .Select(g => g.Name)
                        .ToList(),

                    RecommendationScore =
                        item.RecommendationScore,

                    RecommendationType =
                        recommendationType,

                    Reason =
                        aiResult?.Reason
                        ?? fallbackReason,

                    Confidence =
                        aiResult?.Confidence ?? 0
                };
            })
            .ToList();

        var cacheDuration =
            aiSucceeded
                ? TimeSpan.FromMinutes(5)
                : TimeSpan.FromMinutes(1);

        var cacheOptions =
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(
                    cacheDuration);

        _cache.Set(
            cacheKey,
            recommendations,
            cacheOptions);

        totalStopwatch.Stop();

        _logger.LogInformation(
            "Recommendation request completed in {ElapsedMs} ms. AIUsed: {AIUsed}, RecommendationsCount: {Count}",
            totalStopwatch.ElapsedMilliseconds,
            aiSucceeded,
            recommendations.Count);

        return recommendations;
    }

    private static string GenerateFallbackReason(
        List<string> movieGenres,
        List<string> preferredGenres)
    {
        var matchedGenres = movieGenres
            .Where(movieGenre =>
                preferredGenres.Contains(
                    movieGenre,
                    StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (matchedGenres.Any())
        {
            return
                $"Recommended based on your preference for {string.Join(", ", matchedGenres)}.";
        }

        return
            "A discovery pick outside your usual genres, giving you something different to explore.";
    }
}
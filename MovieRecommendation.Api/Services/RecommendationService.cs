using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Recommendations;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;

    public RecommendationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecommendationResponseDto>> GetRecommendationsAsync(
    string userId)
    {
        var highRatedMovies = await _context.Ratings
            .Where(r => r.UserId == userId && r.Score >= 7)
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

        // Build genre preference scores
        var genreScores = new Dictionary<int, double>();

        foreach (var rating in highRatedMovies)
        {
            foreach (var genre in rating.Movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                }

                genreScores[genre.Id] += 3;
            }
        }

        foreach (var movie in favoriteMovies)
        {
            foreach (var genre in movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                }

                genreScores[genre.Id] += 2;
            }
        }

        foreach (var movie in watchlistMovies)
        {
            foreach (var genre in movie.Genres)
            {
                if (!genreScores.ContainsKey(genre.Id))
                {
                    genreScores[genre.Id] = 0;
                }

                genreScores[genre.Id] += 1;
            }
        }

        // Movies the user already interacted with
        var userMovieIds = highRatedMovies
            .Select(r => r.MovieId)
            .Concat(favoriteMovies.Select(m => m.Id))
            .Concat(watchlistMovies.Select(m => m.Id))
            .Distinct()
            .ToList();

        // Candidate movies
        var candidateMovies = await _context.Movies
            .Where(m =>
                !m.IsDeleted &&
                !userMovieIds.Contains(m.Id))
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .ToListAsync();

        var recommendations = candidateMovies
    .Select(movie =>
    {
        var matchedGenres = movie.Genres
            .Where(g => genreScores.ContainsKey(g.Id))
            .Select(g => g.Name)
            .ToList();

        var genreScore = movie.Genres
            .Where(g => genreScores.ContainsKey(g.Id))
            .Sum(g => genreScores[g.Id]);

        var averageRating = movie.Ratings.Any()
            ? movie.Ratings.Average(r => r.Score)
            : 0;

        var recommendationScore =
            genreScore + (averageRating * 0.2);

        return new RecommendationResponseDto
        {
            MovieId = movie.Id,
            Title = movie.Title,
            PosterUrl = movie.PosterUrl,
            AverageRating = averageRating,
            Genres = movie.Genres
                .Select(g => g.Name)
                .ToList(),
            RecommendationScore = recommendationScore,
            Reason = matchedGenres.Any()
                ? $"Recommended because you enjoy {string.Join(", ", matchedGenres)}."
                : "Recommended based on your preferences."
        };
    })
    .OrderByDescending(r => r.RecommendationScore)
    .Take(10)
    .ToList();

        return recommendations;
    }
}
using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.DTOs.Watchlists;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class WatchlistService : IWatchlistService
{
    private readonly ApplicationDbContext _context;

    public WatchlistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieResponseDto> AddAsync(
        string userId,
        AddWatchlistDto dto)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .FirstOrDefaultAsync(m =>
                m.Id == dto.MovieId &&
                !m.IsDeleted);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        var existingWatchlist = await _context.Watchlists
            .AnyAsync(w =>
                w.UserId == userId &&
                w.MovieId == dto.MovieId);

        if (existingWatchlist)
        {
            throw new InvalidOperationException(
                "Movie is already in your watchlist.");
        }

        var watchlist = new Watchlist
        {
            UserId = userId,
            MovieId = dto.MovieId,
            AddedAt = DateTime.UtcNow
        };

        _context.Watchlists.Add(watchlist);

        await _context.SaveChangesAsync();

        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            ReleaseDate = movie.ReleaseDate,
            Language = movie.Language,
            AgeRating = movie.AgeRating,
            PosterUrl = movie.PosterUrl,
            Directors = movie.Directors,
            CastMembers = movie.CastMembers,
            AverageRating = movie.Ratings.Any()
                ? movie.Ratings.Average(r => r.Score)
                : 0,
            Genres = movie.Genres
                .Select(g => g.Name)
                .ToList()
        };
    }

    public async Task<bool> RemoveAsync(string userId, int movieId)
    {
        var watchlist = await _context.Watchlists
            .FirstOrDefaultAsync(w =>
                w.UserId == userId &&
                w.MovieId == movieId);

        if (watchlist == null)
        {
            return false;
        }

        _context.Watchlists.Remove(watchlist);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<MovieResponseDto>> GetMyWatchlistAsync(
        string userId)
    {
        return await _context.Watchlists
            .Where(w => w.UserId == userId && !w.Movie.IsDeleted)
            .Include(w => w.Movie)
                .ThenInclude(m => m.Genres)
            .Include(w => w.Movie)
                .ThenInclude(m => m.Ratings)
            .Select(w => new MovieResponseDto
            {
                Id = w.Movie.Id,
                Title = w.Movie.Title,
                Description = w.Movie.Description,
                Duration = w.Movie.Duration,
                ReleaseDate = w.Movie.ReleaseDate,
                Language = w.Movie.Language,
                AgeRating = w.Movie.AgeRating,
                PosterUrl = w.Movie.PosterUrl,
                Directors = w.Movie.Directors,
                CastMembers = w.Movie.CastMembers,

                AverageRating = w.Movie.Ratings.Any()
                    ? w.Movie.Ratings.Average(r => r.Score)
                    : 0,

                Genres = w.Movie.Genres
                    .Select(g => g.Name)
                    .ToList()
            })
            .ToListAsync();
    }
}
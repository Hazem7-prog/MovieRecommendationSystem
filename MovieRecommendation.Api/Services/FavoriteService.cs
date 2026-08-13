using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Favorites;
using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;

    public FavoriteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieResponseDto> AddAsync(
        string userId,
        AddFavoriteDto dto)
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

        var existingFavorite = await _context.Favorites
            .AnyAsync(f =>
                f.UserId == userId &&
                f.MovieId == dto.MovieId);

        if (existingFavorite)
        {
            throw new InvalidOperationException(
                "Movie is already in your favorites.");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            MovieId = dto.MovieId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(favorite);

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
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.MovieId == movieId);

        if (favorite == null)
        {
            return false;
        }

        _context.Favorites.Remove(favorite);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<MovieResponseDto>> GetMyFavoritesAsync(
        string userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId && !f.Movie.IsDeleted)
            .Include(f => f.Movie)
                .ThenInclude(m => m.Genres)
            .Include(f => f.Movie)
                .ThenInclude(m => m.Ratings)
            .Select(f => new MovieResponseDto
            {
                Id = f.Movie.Id,
                Title = f.Movie.Title,
                Description = f.Movie.Description,
                Duration = f.Movie.Duration,
                ReleaseDate = f.Movie.ReleaseDate,
                Language = f.Movie.Language,
                AgeRating = f.Movie.AgeRating,
                PosterUrl = f.Movie.PosterUrl,
                Directors = f.Movie.Directors,
                CastMembers = f.Movie.CastMembers,

                AverageRating = f.Movie.Ratings.Any()
                    ? f.Movie.Ratings.Average(r => r.Score)
                    : 0,

                Genres = f.Movie.Genres
                    .Select(g => g.Name)
                    .ToList()
            })
            .ToListAsync();
    }
}
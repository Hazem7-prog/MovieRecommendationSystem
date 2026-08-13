using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Ratings;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RatingResponseDto> CreateAsync(
        string userId,
        CreateRatingDto dto)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == dto.MovieId && !m.IsDeleted);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        if (dto.Score < 1 || dto.Score > 10)
        {
            throw new ArgumentException("Rating score must be between 1 and 10.");
        }

        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.MovieId == dto.MovieId);

        if (existingRating != null)
        {
            throw new InvalidOperationException(
                "You have already rated this movie.");
        }

        var rating = new Rating
        {
            UserId = userId,
            MovieId = dto.MovieId,
            Score = dto.Score,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);

        await _context.SaveChangesAsync();

        return new RatingResponseDto
        {
            Id = rating.Id,
            MovieId = movie.Id,
            MovieTitle = movie.Title,
            Score = rating.Score,
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<List<RatingResponseDto>> GetMyRatingsAsync(string userId)
    {
        return await _context.Ratings
            .Where(r => r.UserId == userId)
            .Include(r => r.Movie)
            .Select(r => new RatingResponseDto
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie.Title,
                Score = r.Score,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
}
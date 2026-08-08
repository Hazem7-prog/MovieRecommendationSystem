using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class MovieService : IMovieService
{
    private readonly ApplicationDbContext _context;

    public MovieService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieResponseDto> CreateAsync(CreateMovieDto dto)
    {
        var genres = await _context.Genres
            .Where(g => dto.GenreIds.Contains(g.Id))
            .ToListAsync();

        var movie = new Movie
        {
            Title = dto.Title,
            Description = dto.Description,
            Duration = dto.Duration,
            ReleaseDate = dto.ReleaseDate,
            Language = dto.Language,
            AgeRating = dto.AgeRating,
            PosterUrl = dto.PosterUrl,
            Directors = dto.Directors,
            CastMembers = dto.CastMembers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Genres = genres
        };

        _context.Movies.Add(movie);

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
            Genres = movie.Genres
                .Select(g => g.Name)
                .ToList()
        };
    }

    public Task<MovieResponseDto?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<MovieResponseDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(int id, UpdateMovieDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}
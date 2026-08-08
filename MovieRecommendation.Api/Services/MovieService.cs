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

    public async Task<MovieResponseDto?> GetByIdAsync(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (movie == null)
        {
            return null;
        }

        var averageRating = movie.Ratings.Any()
            ? movie.Ratings.Average(r => r.Score)
            : 0;

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
            AverageRating = averageRating,
            Genres = movie.Genres
                .Select(g => g.Name)
                .ToList()
        };
    }

    public async Task<List<MovieResponseDto>> GetAllAsync()
    {
        var movies = await _context.Movies
            .Where(m => !m.IsDeleted)
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .ToListAsync();

        return movies.Select(movie => new MovieResponseDto
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
        }).ToList();
    }

    public async Task<bool> UpdateAsync(int id, UpdateMovieDto dto)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (movie == null)
        {
            return false;
        }

        var genres = await _context.Genres
            .Where(g => dto.GenreIds.Contains(g.Id))
            .ToListAsync();

        movie.Title = dto.Title;
        movie.Description = dto.Description;
        movie.Duration = dto.Duration;
        movie.ReleaseDate = dto.ReleaseDate;
        movie.Language = dto.Language;
        movie.AgeRating = dto.AgeRating;
        movie.PosterUrl = dto.PosterUrl;
        movie.Directors = dto.Directors;
        movie.CastMembers = dto.CastMembers;
        movie.UpdatedAt = DateTime.UtcNow;

        movie.Genres.Clear();

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (movie == null)
        {
            return false;
        }

        movie.IsDeleted = true;
        movie.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
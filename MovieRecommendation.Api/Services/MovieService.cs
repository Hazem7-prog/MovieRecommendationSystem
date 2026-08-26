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

    public async Task<PagedResultDto<MovieResponseDto>> GetAllAsync(
        MovieQueryDto query)
    {
        IQueryable<Movie> movies = _context.Movies
            .Where(m => !m.IsDeleted);

        // Search by title
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            movies = movies.Where(m =>
                m.Title.Contains(query.Search));
        }

        // Filter by genre
        if (query.GenreId.HasValue)
        {
            movies = movies.Where(m =>
                m.Genres.Any(g => g.Id == query.GenreId.Value));
        }

        // Filter by language
        if (!string.IsNullOrWhiteSpace(query.Language))
        {
            movies = movies.Where(m =>
                m.Language == query.Language);
        }

        // Filter by release year
        if (query.Year.HasValue)
        {
            movies = movies.Where(m =>
                m.ReleaseDate.Year == query.Year.Value);
        }

        // Filter by minimum average rating
        if (query.MinRating.HasValue)
        {
            movies = movies.Where(m =>
                m.Ratings.Any() &&
                m.Ratings.Average(r => r.Score) >= query.MinRating.Value);
        }

        // Sorting
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            switch (query.SortBy.ToLower())
            {
                case "title":
                    movies = movies.OrderBy(m => m.Title);
                    break;

                case "releasedate":
                    movies = movies.OrderByDescending(m => m.ReleaseDate);
                    break;

                case "rating":
                    movies = movies.OrderByDescending(m =>
                        m.Ratings.Any()
                            ? m.Ratings.Average(r => r.Score)
                            : 0);
                    break;

                default:
                    movies = movies.OrderBy(m => m.Id);
                    break;
            }
        }
        else
        {
            movies = movies.OrderBy(m => m.Id);
        }

        // Pagination settings are validated by MovieQueryDto DataAnnotations (PageNumber >= 1, PageSize 1-50)
        var pageSize = query.PageSize;

        var pageNumber = query.PageNumber;

        // Total count before pagination
        var totalCount = await movies.CountAsync();

        // Pagination
        movies = movies
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        // Execute query and map to DTO
        var result = await movies
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .Select(movie => new MovieResponseDto
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
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);

        return new PagedResultDto<MovieResponseDto>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateMovieDto dto)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .FirstOrDefaultAsync(m =>
                m.Id == id &&
                !m.IsDeleted);

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
            .FirstOrDefaultAsync(m =>
                m.Id == id &&
                !m.IsDeleted);

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
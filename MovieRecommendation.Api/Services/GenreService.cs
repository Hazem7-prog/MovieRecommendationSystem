using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Genres;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class GenreService : IGenreService
{
    private readonly ApplicationDbContext _context;

    public GenreService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GenreResponseDto> CreateAsync(CreateGenreDto dto)
    {
        var genre = new Genre
        {
            Name = dto.Name
        };

        _context.Genres.Add(genre);

        await _context.SaveChangesAsync();

        return new GenreResponseDto
        {
            Id = genre.Id,
            Name = genre.Name
        };
    }

    public async Task<List<GenreResponseDto>> GetAllAsync()
    {
        return await _context.Genres
            .Select(g => new GenreResponseDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToListAsync();
    }
}
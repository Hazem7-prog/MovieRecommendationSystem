using MovieRecommendation.Api.DTOs.Genres;

namespace MovieRecommendation.Api.Interfaces;

public interface IGenreService
{
    Task<GenreResponseDto> CreateAsync(CreateGenreDto dto);

    Task<List<GenreResponseDto>> GetAllAsync();
}
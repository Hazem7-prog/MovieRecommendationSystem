using MovieRecommendation.Api.DTOs.Movies;

namespace MovieRecommendation.Api.Interfaces;

public interface IMovieService
{
    Task<MovieResponseDto> CreateAsync(CreateMovieDto dto);

    Task<MovieResponseDto?> GetByIdAsync(int id);

    Task<List<MovieResponseDto>> GetAllAsync();

    Task<bool> UpdateAsync(int id, UpdateMovieDto dto);

    Task<bool> DeleteAsync(int id);
}
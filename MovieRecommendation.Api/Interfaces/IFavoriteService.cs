using MovieRecommendation.Api.DTOs.Favorites;
using MovieRecommendation.Api.DTOs.Movies;

namespace MovieRecommendation.Api.Interfaces;

public interface IFavoriteService
{
    Task<MovieResponseDto> AddAsync(string userId, AddFavoriteDto dto);

    Task<bool> RemoveAsync(string userId, int movieId);

    Task<List<MovieResponseDto>> GetMyFavoritesAsync(string userId);
}
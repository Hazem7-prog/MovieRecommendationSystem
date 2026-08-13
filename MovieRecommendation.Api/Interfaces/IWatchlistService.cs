using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.DTOs.Watchlists;

namespace MovieRecommendation.Api.Interfaces;

public interface IWatchlistService
{
    Task<MovieResponseDto> AddAsync(string userId, AddWatchlistDto dto);

    Task<bool> RemoveAsync(string userId, int movieId);

    Task<List<MovieResponseDto>> GetMyWatchlistAsync(string userId);
}
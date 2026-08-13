using MovieRecommendation.Api.DTOs.Ratings;

namespace MovieRecommendation.Api.Interfaces;

public interface IRatingService
{
    Task<RatingResponseDto> CreateAsync(string userId, CreateRatingDto dto);

    Task<List<RatingResponseDto>> GetMyRatingsAsync(string userId);
}
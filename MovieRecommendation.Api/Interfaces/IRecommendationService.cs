using MovieRecommendation.Api.DTOs.ExternalRecommendations;
using MovieRecommendation.Api.DTOs.Recommendations;

namespace MovieRecommendation.Api.Interfaces;

public interface IRecommendationService
{
    Task<List<RecommendationResponseDto>> GetRecommendationsAsync(
        string userId);

    Task<List<ExternalRecommendationResponseDto>> GetExternalRecommendationsAsync(
        ExternalRecommendationRequestDto request);
}
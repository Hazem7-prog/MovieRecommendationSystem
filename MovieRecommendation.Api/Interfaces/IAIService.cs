using MovieRecommendation.Api.DTOs.AI;

namespace MovieRecommendation.Api.Interfaces;

public interface IAIService
{
    Task<List<AIRecommendationExplanationDto>>
        GenerateRecommendationExplanationsAsync(
            string userPreferences,
            string userInteractions,
            List<MovieForAIRequestDto> movies);
}
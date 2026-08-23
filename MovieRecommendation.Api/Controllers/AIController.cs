using Microsoft.AspNetCore.Mvc;
using MovieRecommendation.Api.DTOs.AI;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("explain")]
    public async Task<IActionResult> Explain(
        AIExplanationRequestDto dto)
    {
        var movies = new List<MovieForAIRequestDto>
        {
            new MovieForAIRequestDto
            {
                MovieId = dto.MovieId,
                Title = dto.MovieTitle,
                Genres = dto.MovieGenres
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .ToList()
            }
        };

        var result = await _aiService
            .GenerateRecommendationExplanationsAsync(
                dto.UserPreferences,
                dto.UserInteractions,
                movies);

        return Ok(result);
    }
}
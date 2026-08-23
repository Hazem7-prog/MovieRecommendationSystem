using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(
        IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    [EnableRateLimiting("recommendations")]
    public async Task<IActionResult> GetRecommendations()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var recommendations =
            await _recommendationService
                .GetRecommendationsAsync(userId);

        return Ok(recommendations);
    }
}
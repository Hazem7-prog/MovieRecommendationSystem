using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MovieRecommendation.Api.DTOs.ExternalRecommendations;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IConfiguration _configuration;

    public RecommendationsController(
        IRecommendationService recommendationService,
        IConfiguration configuration)
    {
        _recommendationService = recommendationService;
        _configuration = configuration;
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

    [HttpPost("external")]
    [AllowAnonymous]
    [EnableRateLimiting("recommendations")]
    public async Task<IActionResult> GetExternalRecommendations(
        [FromHeader(Name = "X-API-Key")] string apiKey,
        [FromBody] ExternalRecommendationRequestDto request)
    {
        var expectedApiKey =
            _configuration["ExternalApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey) ||
            apiKey != expectedApiKey)
        {
            return Unauthorized(new
            {
                message = "Invalid API key."
            });
        }

        var recommendations =
            await _recommendationService
                .GetExternalRecommendationsAsync(request);

        return Ok(recommendations);
    }
}
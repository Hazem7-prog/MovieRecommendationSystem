using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRecommendation.Api.DTOs.Favorites;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddFavoriteDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var movie = await _favoriteService.AddAsync(userId, dto);

            return Ok(movie);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{movieId}")]
    public async Task<IActionResult> Remove(int movieId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _favoriteService.RemoveAsync(userId, movieId);

        if (!result)
        {
            return NotFound(new
            {
                message = "Movie is not in your favorites."
            });
        }

        return Ok(new
        {
            message = "Movie removed from favorites."
        });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var movies = await _favoriteService.GetMyFavoritesAsync(userId);

        return Ok(movies);
    }
}
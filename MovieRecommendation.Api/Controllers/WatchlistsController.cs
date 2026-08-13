using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRecommendation.Api.DTOs.Watchlists;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WatchlistsController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistsController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddWatchlistDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var movie = await _watchlistService.AddAsync(userId, dto);

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

        var result = await _watchlistService.RemoveAsync(userId, movieId);

        if (!result)
        {
            return NotFound(new
            {
                message = "Movie is not in your watchlist."
            });
        }

        return Ok(new
        {
            message = "Movie removed from watchlist."
        });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyWatchlist()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var movies = await _watchlistService.GetMyWatchlistAsync(userId);

        return Ok(movies);
    }
}
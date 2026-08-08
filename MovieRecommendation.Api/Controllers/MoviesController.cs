using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMovieDto dto)
    {
        var movie = await _movieService.CreateAsync(dto);

        return Ok(movie);
    }
}
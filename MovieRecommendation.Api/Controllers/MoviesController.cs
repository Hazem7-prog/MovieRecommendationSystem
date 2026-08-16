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


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);

        if (movie == null)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(movie);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MovieQueryDto query)
    {
        var movies = await _movieService.GetAllAsync(query);

        return Ok(movies);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMovieDto dto)
    {
        var result = await _movieService.UpdateAsync(id, dto);

        if (!result)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(new
        {
            message = "Movie updated successfully."
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _movieService.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(new
        {
            message = "Movie deleted successfully."
        });
    }
}
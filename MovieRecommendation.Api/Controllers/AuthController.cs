using Microsoft.AspNetCore.Mvc;
using MovieRecommendation.Api.DTOs.Auth;
using MovieRecommendation.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MovieRecommendation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new
        {
            UserId = userId,
            Email = email,
            FullName = fullName
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-test")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            Message = "Welcome Admin 👑",
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}
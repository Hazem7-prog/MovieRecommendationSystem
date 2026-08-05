using Microsoft.AspNetCore.Identity;
using MovieRecommendation.Api.DTOs.Auth;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return "User registered successfully.";
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        throw new NotImplementedException();
    }
}
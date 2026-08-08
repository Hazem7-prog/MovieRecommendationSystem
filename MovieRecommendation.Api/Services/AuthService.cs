using Microsoft.AspNetCore.Identity;
using MovieRecommendation.Api.DTOs.Auth;
using MovieRecommendation.Api.DTOs.Responses;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Services;

public class AuthService : IAuthService

{
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(
        ITokenService tokenService, 
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
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
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }
        await _userManager.AddToRoleAsync(user, "User");

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "User registered successfully.",
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid email or password."
            };
        }
        var jwt = await _tokenService.CreateTokenAsync(user);
        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful.",
            Email = user.Email,
            FullName = user.FullName,
            Token = jwt.Token,
            ExpireAt = jwt.ExpireAt
        };
    }
}
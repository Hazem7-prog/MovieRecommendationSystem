using MovieRecommendation.Api.DTOs.Auth;
using MovieRecommendation.Api.DTOs.Responses;

namespace MovieRecommendation.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
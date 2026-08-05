using MovieRecommendation.Api.DTOs.Auth;

namespace MovieRecommendation.Api.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);

    Task<string> LoginAsync(LoginDto dto);
}
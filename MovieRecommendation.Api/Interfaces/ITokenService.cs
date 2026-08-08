using MovieRecommendation.Api.DTOs.Responses;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Interfaces;

public interface ITokenService
{
    Task<JwtTokenDto> CreateTokenAsync(ApplicationUser user);

}
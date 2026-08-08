namespace MovieRecommendation.Api.DTOs.Responses;

public class JwtTokenDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpireAt { get; set; }
}
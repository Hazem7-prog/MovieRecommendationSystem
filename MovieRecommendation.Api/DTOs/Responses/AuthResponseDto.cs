namespace MovieRecommendation.Api.DTOs.Responses;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Token { get; set; }

    public DateTime? ExpireAt { get; set; }

    public string? Email { get; set; }

    public string? FullName { get; set; }
}
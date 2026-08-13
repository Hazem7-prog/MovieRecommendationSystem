namespace MovieRecommendation.Api.DTOs.Ratings;

public class RatingResponseDto
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public string MovieTitle { get; set; } = string.Empty;

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }
}
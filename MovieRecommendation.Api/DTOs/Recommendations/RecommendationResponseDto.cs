namespace MovieRecommendation.Api.DTOs.Recommendations;

public class RecommendationResponseDto
{
    public int MovieId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public List<string> Genres { get; set; } = new();

    public double RecommendationScore { get; set; }

    public string RecommendationType { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public double Confidence { get; set; }
}
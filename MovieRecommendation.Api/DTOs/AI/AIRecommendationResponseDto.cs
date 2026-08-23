namespace MovieRecommendation.Api.DTOs.AI;

public class AIRecommendationResponseDto
{
    public string Reason { get; set; } = string.Empty;

    public double Confidence { get; set; }
}
namespace MovieRecommendation.Api.DTOs.AI;

public class AIRecommendationExplanationDto
{
    public int MovieId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double Confidence { get; set; }
}
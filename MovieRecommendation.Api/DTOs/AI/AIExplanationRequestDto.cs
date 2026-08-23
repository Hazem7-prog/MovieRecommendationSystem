namespace MovieRecommendation.Api.DTOs.AI;

public class AIExplanationRequestDto
{
    public int MovieId { get; set; }

    public string UserPreferences { get; set; } = string.Empty;

    public string UserInteractions { get; set; } = string.Empty;

    public string MovieTitle { get; set; } = string.Empty;

    public string MovieGenres { get; set; } = string.Empty;
}
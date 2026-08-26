using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.AI;

public class AIExplanationRequestDto
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }

    [MaxLength(2000)]
    public string UserPreferences { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string UserInteractions { get; set; } = string.Empty;

    [MaxLength(250)]
    public string MovieTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MovieGenres { get; set; } = string.Empty;
}

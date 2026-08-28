using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.ExternalRecommendations;

public class ExternalRecommendationRequestDto
{
    [Required]
    [MaxLength(100)]
    public List<ExternalMediaInteractionDto> Favorites { get; set; } = new();

    [Required]
    [MaxLength(100)]
    public List<ExternalMediaInteractionDto> WatchLater { get; set; } = new();
}
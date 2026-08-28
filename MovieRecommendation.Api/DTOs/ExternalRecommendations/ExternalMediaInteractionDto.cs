using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.ExternalRecommendations;

public class ExternalMediaInteractionDto
{
    [Required]
    [MaxLength(50)]
    public string Id { get; set; } = string.Empty;

    [Range(0, 10)]
    public double Rating { get; set; }

    [Required]
    [RegularExpression(
        "^(tmdb|imdb)$",
        ErrorMessage = "Source must be either 'tmdb' or 'imdb'.")]
    public string Source { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    [MaxLength(20)]
    public List<string> Genres { get; set; } = new();
}
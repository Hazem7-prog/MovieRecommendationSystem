using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.AI;

public class MovieForAIRequestDto
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    public List<string> Genres { get; set; } = new();
}

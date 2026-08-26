using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Genres;

public class CreateGenreDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

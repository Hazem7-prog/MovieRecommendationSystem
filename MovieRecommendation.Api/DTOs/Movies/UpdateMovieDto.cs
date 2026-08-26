using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Movies;

public class UpdateMovieDto
{
    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Duration { get; set; }

    [Required]
    public DateOnly ReleaseDate { get; set; }

    [MaxLength(100)]
    public string Language { get; set; } = string.Empty;

    [MaxLength(10)]
    public string AgeRating { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Url]
    public string PosterUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Directors { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string CastMembers { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<int> GenreIds { get; set; } = new List<int>();
}   

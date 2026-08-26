using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Movies;

public class MovieQueryDto
{
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int? GenreId { get; set; }

    public string? Language { get; set; }

    public int? Year { get; set; }

    public double? MinRating { get; set; }

    public string? SortBy { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}

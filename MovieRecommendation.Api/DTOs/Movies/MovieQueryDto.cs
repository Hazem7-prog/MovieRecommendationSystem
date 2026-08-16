namespace MovieRecommendation.Api.DTOs.Movies;

public class MovieQueryDto
{
    public string? Search { get; set; }

    public int? GenreId { get; set; }

    public string? Language { get; set; }

    public int? Year { get; set; }

    public double? MinRating { get; set; }

    public string? SortBy { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
namespace MovieRecommendation.Api.Models;

public class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Duration { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string Language { get; set; } = string.Empty;

    public string AgeRating { get; set; } = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;

    public string Director { get; set; } = string.Empty;

    public string Cast { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
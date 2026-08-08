namespace MovieRecommendation.Api.DTOs.Movies;

public class CreateMovieDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Duration { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string Language { get; set; } = string.Empty;

    public string AgeRating { get; set; } = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;

    public string Directors { get; set; } = string.Empty;

    public string CastMembers { get; set; } = string.Empty;

    public List<int> GenreIds { get; set; } = [];
}
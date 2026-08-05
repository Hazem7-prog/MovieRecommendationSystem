namespace MovieRecommendation.Api.Models;

public class Watchlist
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public int MovieId { get; set; }

    public Movie Movie { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}
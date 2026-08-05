namespace MovieRecommendation.Api.Models;

public class Rating
{
    public int Id { get; set; }

    public int Score { get; set; }

    public int MovieId { get; set; }

    public Movie Movie { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
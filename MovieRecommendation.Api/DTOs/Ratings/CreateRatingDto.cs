namespace MovieRecommendation.Api.DTOs.Ratings;

public class CreateRatingDto
{
    public int MovieId { get; set; }

    public int Score { get; set; }
}
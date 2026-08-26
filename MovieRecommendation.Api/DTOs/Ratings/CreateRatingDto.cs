using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Ratings;

public class CreateRatingDto
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }

    [Range(1, 10)]
    public int Score { get; set; }
}

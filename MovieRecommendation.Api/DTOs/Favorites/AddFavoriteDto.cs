using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Favorites;

public class AddFavoriteDto
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }
}

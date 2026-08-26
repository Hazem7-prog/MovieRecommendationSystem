using System.ComponentModel.DataAnnotations;

namespace MovieRecommendation.Api.DTOs.Watchlists;

public class AddWatchlistDto
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }
}

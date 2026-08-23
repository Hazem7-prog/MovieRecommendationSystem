namespace MovieRecommendation.Api.DTOs.AI;

public class MovieForAIRequestDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();
}
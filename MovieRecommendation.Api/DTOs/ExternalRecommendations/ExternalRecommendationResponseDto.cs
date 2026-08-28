namespace MovieRecommendation.Api.DTOs.ExternalRecommendations
{
    public class ExternalRecommendationResponseDto
    {
        public string Key { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string PosterUrl { get; set; } = string.Empty;

        public double Rating { get; set; }

        public List<string> Genres { get; set; } = new();

        public double RecommendationScore { get; set; }

        public string RecommendationType { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
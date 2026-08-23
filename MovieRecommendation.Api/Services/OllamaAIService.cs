using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using MovieRecommendation.Api.DTOs.AI;
using MovieRecommendation.Api.Interfaces;

namespace MovieRecommendation.Api.Services;

public class OllamaAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaAIService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaAIService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OllamaAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<AIRecommendationExplanationDto>>
        GenerateRecommendationExplanationsAsync(
            string userPreferences,
            string userInteractions,
            List<MovieForAIRequestDto> movies)
    {
        var stopwatch = Stopwatch.StartNew();

        var model =
            _configuration["Ollama:Model"]
            ?? "qwen3:4b";

        _logger.LogInformation(
            "Starting Ollama batch request. Model: {Model}, MoviesCount: {MoviesCount}",
            model,
            movies.Count);

        try
        {
            var moviesJson =
                JsonSerializer.Serialize(movies);

            var prompt = $$"""
                You are a movie recommendation assistant.

                User preferred genres:
                {{userPreferences}}

                Verified user interactions:
                {{userInteractions}}

                Movies to explain:
                {{moviesJson}}

                For each movie, explain briefly why it is a good or bad recommendation
                based ONLY on the verified user information and the movie genres.

                IMPORTANT RULES:
                - Do not invent user ratings.
                - Do not invent favorites.
                - Do not invent watchlist items.
                - Do not claim that the user interacted with a movie unless it appears
                  in the verified user interactions.
                - Use only the information provided.
                - Keep each explanation short.
                - Return exactly one result for every movie.
                - Keep the same movieId provided in the input.

                Return ONLY valid JSON in this exact structure:

                [
                  {
                    "movieId": 0,
                    "reason": "short explanation",
                    "confidence": 0.0
                  }
                ]

                Confidence must be a number between 0 and 1.
                """;

            var request = new
            {
                model,

                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },

                stream = false,

                format = new
                {
                    type = "array",

                    items = new
                    {
                        type = "object",

                        properties = new
                        {
                            movieId = new
                            {
                                type = "integer"
                            },

                            reason = new
                            {
                                type = "string"
                            },

                            confidence = new
                            {
                                type = "number"
                            }
                        },

                        required = new[]
                        {
                            "movieId",
                            "reason",
                            "confidence"
                        }
                    }
                }
            };

            using var cancellationTokenSource =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(20));

            var response =
                await _httpClient.PostAsJsonAsync(
                    "api/chat",
                    request,
                    cancellationTokenSource.Token);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Ollama returned {(int)response.StatusCode}: {responseContent}");
            }

            var ollamaResponse =
                JsonSerializer.Deserialize<OllamaChatResponse>(
                    responseContent,
                    JsonOptions);

            if (ollamaResponse?.Message?.Content is null ||
                string.IsNullOrWhiteSpace(
                    ollamaResponse.Message.Content))
            {
                throw new Exception(
                    "Ollama returned an empty message content.");
            }

            var aiResults =
                JsonSerializer.Deserialize<
                    List<AIRecommendationExplanationDto>>(
                        ollamaResponse.Message.Content,
                        JsonOptions);

            if (aiResults is null)
            {
                throw new Exception(
                    "Failed to deserialize AI recommendation explanations.");
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "Ollama batch request completed successfully in {ElapsedMs} ms. ResultsCount: {ResultsCount}",
                stopwatch.ElapsedMilliseconds,
                aiResults.Count);

            return aiResults;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                ex,
                "Ollama batch request failed after {ElapsedMs} ms.",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }
    }

    private class OllamaMessage
    {
        public string? Role { get; set; }

        public string? Content { get; set; }
    }
}
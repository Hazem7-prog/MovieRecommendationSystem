using MovieRecommendation.Api.DTOs.ExternalRecommendations;

namespace MovieRecommendation.Api.Interfaces;

public interface ITmdbService
{
    Task<Dictionary<string, int>> GetMovieGenresAsync();

    Task<List<TmdbMovieCandidateDto>> GetMovieCandidatesAsync(
        IEnumerable<int> genreIds);

    Task<int?> ResolveTmdbMovieIdAsync(
        string id,
        string source);
}
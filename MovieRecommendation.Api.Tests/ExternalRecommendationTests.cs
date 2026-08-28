using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.ExternalRecommendations;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Services;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class ExternalRecommendationTests
{
    private static RecommendationService CreateService(
        Mock<ITmdbService> tmdbMock)
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        var context =
            new ApplicationDbContext(options);

        var aiMock =
            new Mock<IAIService>();

        var cache =
            new MemoryCache(
                new MemoryCacheOptions());

        var logger =
            NullLogger<RecommendationService>.Instance;

        return new RecommendationService(
            context,
            aiMock.Object,
            cache,
            logger,
            tmdbMock.Object);
    }

    [Fact]
    public async Task ExternalRecommendations_ShouldReturnPersonalizedMovies()
    {
        // Arrange
        var tmdbMock =
            new Mock<ITmdbService>();

        tmdbMock
            .Setup(x =>
                x.GetMovieGenresAsync())
            .ReturnsAsync(
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["Science Fiction"] = 878,
                    ["Thriller"] = 53,
                    ["Action"] = 28
                });

        tmdbMock
            .Setup(x =>
                x.GetMovieCandidatesAsync(
                    It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(
                new List<TmdbMovieCandidateDto>
                {
                    new()
                    {
                        Id = 100,
                        Title = "Sci-Fi Thriller",
                        VoteAverage = 8,
                        GenreIds =
                        {
                            878,
                            53
                        }
                    },

                    new()
                    {
                        Id = 200,
                        Title = "Sci-Fi Movie",
                        VoteAverage = 9,
                        GenreIds =
                        {
                            878
                        }
                    }
                });

        tmdbMock
            .Setup(x =>
                x.ResolveTmdbMovieIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync((int?)null);

        var service =
            CreateService(tmdbMock);

        var request =
            new ExternalRecommendationRequestDto
            {
                Favorites =
                {
                    new ExternalMediaInteractionDto
                    {
                        Id = "999",
                        Source = "tmdb",
                        Rating = 8,
                        Genres =
                        {
                            "Sci-Fi",
                            "Thriller"
                        }
                    }
                }
            };

        // Act
        var result =
            await service
                .GetExternalRecommendationsAsync(
                    request);

        // Assert
        Assert.NotEmpty(result);

        Assert.Equal(
            "Sci-Fi Thriller",
            result.First().Title);

        Assert.Equal(
            "Personalized",
            result.First().RecommendationType);

        Assert.Contains(
            "Science Fiction",
            result.First().Genres);

        Assert.Contains(
            "Thriller",
            result.First().Genres);
    }

    [Fact]
    public async Task ExternalRecommendations_ShouldExcludeAlreadyInteractedMovie()
    {
        // Arrange
        var tmdbMock =
            new Mock<ITmdbService>();

        tmdbMock
            .Setup(x =>
                x.GetMovieGenresAsync())
            .ReturnsAsync(
                new Dictionary<string, int>
                {
                    ["Science Fiction"] = 878
                });

        tmdbMock
            .Setup(x =>
                x.GetMovieCandidatesAsync(
                    It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(
                new List<TmdbMovieCandidateDto>
                {
                    new()
                    {
                        Id = 157336,
                        Title = "Already Watched Movie",
                        VoteAverage = 10,
                        GenreIds =
                        {
                            878
                        }
                    },

                    new()
                    {
                        Id = 200,
                        Title = "New Movie",
                        VoteAverage = 8,
                        GenreIds =
                        {
                            878
                        }
                    }
                });

        tmdbMock
            .Setup(x =>
                x.ResolveTmdbMovieIdAsync(
                    "157336",
                    "tmdb"))
            .ReturnsAsync(
                157336);

        var service =
            CreateService(tmdbMock);

        var request =
            new ExternalRecommendationRequestDto
            {
                Favorites =
                {
                    new ExternalMediaInteractionDto
                    {
                        Id = "157336",
                        Source = "tmdb",
                        Rating = 9,
                        Genres =
                        {
                            "Sci-Fi"
                        }
                    }
                }
            };

        // Act
        var result =
            await service
                .GetExternalRecommendationsAsync(
                    request);

        // Assert
        Assert.DoesNotContain(
            result,
            x => x.Key == "157336");

        Assert.Contains(
            result,
            x => x.Key == "200");
    }

    [Fact]
    public async Task ExternalRecommendations_ColdStart_ShouldReturnDiscoveryMovies()
    {
        // Arrange
        var tmdbMock =
            new Mock<ITmdbService>();

        tmdbMock
            .Setup(x =>
                x.GetMovieGenresAsync())
            .ReturnsAsync(
                new Dictionary<string, int>
                {
                    ["Drama"] = 18,
                    ["Action"] = 28
                });

        tmdbMock
            .Setup(x =>
                x.GetMovieCandidatesAsync(
                    It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(
                new List<TmdbMovieCandidateDto>
                {
                    new()
                    {
                        Id = 1,
                        Title = "Movie One",
                        VoteAverage = 8.5,
                        GenreIds =
                        {
                            18
                        }
                    },

                    new()
                    {
                        Id = 2,
                        Title = "Movie Two",
                        VoteAverage = 9,
                        GenreIds =
                        {
                            28
                        }
                    }
                });

        var service =
            CreateService(tmdbMock);

        var request =
            new ExternalRecommendationRequestDto();

        // Act
        var result =
            await service
                .GetExternalRecommendationsAsync(
                    request);

        // Assert
        Assert.Equal(
            2,
            result.Count);

        Assert.All(
            result,
            movie =>
                Assert.Equal(
                    "Discovery",
                    movie.RecommendationType));

        Assert.Equal(
            "Movie Two",
            result.First().Title);
    }

    [Fact]
    public async Task ExternalRecommendations_ShouldNormalizeImdbIdAndExcludeMovie()
    {
        // Arrange
        var tmdbMock =
            new Mock<ITmdbService>();

        tmdbMock
            .Setup(x =>
                x.GetMovieGenresAsync())
            .ReturnsAsync(
                new Dictionary<string, int>
                {
                    ["Science Fiction"] = 878
                });

        tmdbMock
            .Setup(x =>
                x.GetMovieCandidatesAsync(
                    It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(
                new List<TmdbMovieCandidateDto>
                {
                    new()
                    {
                        Id = 27205,
                        Title = "Interacted Movie",
                        VoteAverage = 9,
                        GenreIds =
                        {
                            878
                        }
                    },

                    new()
                    {
                        Id = 200,
                        Title = "New Movie",
                        VoteAverage = 8,
                        GenreIds =
                        {
                            878
                        }
                    }
                });

        tmdbMock
            .Setup(x =>
                x.ResolveTmdbMovieIdAsync(
                    "tt1375666",
                    "imdb"))
            .ReturnsAsync(
                27205);

        var service =
            CreateService(tmdbMock);

        var request =
            new ExternalRecommendationRequestDto
            {
                Favorites =
                {
                    new ExternalMediaInteractionDto
                    {
                        Id = "tt1375666",
                        Source = "imdb",
                        Rating = 8.8,
                        Genres =
                        {
                            "Sci-Fi"
                        }
                    }
                }
            };

        // Act
        var result =
            await service
                .GetExternalRecommendationsAsync(
                    request);

        // Assert
        Assert.DoesNotContain(
            result,
            x => x.Key == "27205");

        Assert.Contains(
            result,
            x => x.Key == "200");
    }
}
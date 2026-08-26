using Microsoft.Extensions.Caching.Memory;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Ratings;
using MovieRecommendation.Api.Models;
using MovieRecommendation.Api.Services;
using MovieRecommendation.Api.Tests.Helpers;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class RatingServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRating_Succeeds()
    {
        // Arrange
        var dbName = nameof(CreateAsync_WithValidRating_Succeeds);
        using var ctx = TestHelpers.CreateContext(dbName);

        var movie = TestHelpers.CreateMovie("M1");
        ctx.Movies.Add(movie);
        await ctx.SaveChangesAsync();

        var cache = TestHelpers.CreateMemoryCache();
        var svc = new RatingService(ctx, cache);

        var dto = new CreateRatingDto { MovieId = movie.Id, Score = 8 };

        // Act
        var result = await svc.CreateAsync("user1", dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movie.Id, result.MovieId);
        Assert.Equal(8, result.Score);

        var saved = ctx.Ratings.FirstOrDefault(r => r.MovieId == movie.Id && r.UserId == "user1");
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateAsync_NonExistingMovie_ThrowsKeyNotFoundException()
    {
        var dbName = nameof(CreateAsync_NonExistingMovie_ThrowsKeyNotFoundException);
        using var ctx = TestHelpers.CreateContext(dbName);

        var cache = TestHelpers.CreateMemoryCache();
        var svc = new RatingService(ctx, cache);

        var dto = new CreateRatingDto { MovieId = 9999, Score = 5 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.CreateAsync("user1", dto));
    }

    [Fact]
    public async Task CreateAsync_DuplicateRatingForSameUser_ThrowsInvalidOperationException()
    {
        var dbName = nameof(CreateAsync_DuplicateRatingForSameUser_ThrowsInvalidOperationException);
        using var ctx = TestHelpers.CreateContext(dbName);

        var movie = TestHelpers.CreateMovie("M1");
        ctx.Movies.Add(movie);
        ctx.Ratings.Add(new Rating { Movie = movie, MovieId = movie.Id, UserId = "user1", Score = 7, CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var cache = TestHelpers.CreateMemoryCache();
        var svc = new RatingService(ctx, cache);

        var dto = new CreateRatingDto { MovieId = movie.Id, Score = 9 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateAsync("user1", dto));
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.DTOs.Favorites;
using MovieRecommendation.Api.Models;
using MovieRecommendation.Api.Services;
using MovieRecommendation.Api.Tests.Helpers;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class FavoriteServiceTests
{
    [Fact]
    public async Task AddAsync_WithValidFavorite_Succeeds_AndInvalidatesCache()
    {
        var dbName = nameof(AddAsync_WithValidFavorite_Succeeds_AndInvalidatesCache);
        using var ctx = TestHelpers.CreateContext(dbName);

        var movie = TestHelpers.CreateMovie("F1");
        ctx.Movies.Add(movie);
        await ctx.SaveChangesAsync();

        var cache = TestHelpers.CreateMemoryCache();
        var key = "recommendations_user1";
        cache.Set(key, new List<object> { 1 });

        var svc = new FavoriteService(ctx, cache);

        var dto = new AddFavoriteDto { MovieId = movie.Id };

        var result = await svc.AddAsync("user1", dto);

        Assert.NotNull(result);
        Assert.Equal(movie.Id, result.Id);
        Assert.False(cache.TryGetValue(key, out _));
    }

    [Fact]
    public async Task AddAsync_DuplicateFavorite_ThrowsInvalidOperationException()
    {
        var dbName = nameof(AddAsync_DuplicateFavorite_ThrowsInvalidOperationException);
        using var ctx = TestHelpers.CreateContext(dbName);

        var movie = TestHelpers.CreateMovie("F1");
        ctx.Movies.Add(movie);
        ctx.Favorites.Add(new Favorite { Movie = movie, MovieId = movie.Id, UserId = "user1", CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var cache = TestHelpers.CreateMemoryCache();
        var svc = new FavoriteService(ctx, cache);

        var dto = new AddFavoriteDto { MovieId = movie.Id };

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddAsync("user1", dto));
    }

    [Fact]
    public async Task RemoveAsync_ExistingFavorite_ReturnsTrue_AndInvalidatesCache()
    {
        var dbName = nameof(RemoveAsync_ExistingFavorite_ReturnsTrue_AndInvalidatesCache);
        using var ctx = TestHelpers.CreateContext(dbName);

        var movie = TestHelpers.CreateMovie("F1");
        ctx.Movies.Add(movie);
        ctx.Favorites.Add(new Favorite { Movie = movie, MovieId = movie.Id, UserId = "user1", CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var cache = TestHelpers.CreateMemoryCache();
        var key = "recommendations_user1";
        cache.Set(key, new List<object> { 1 });

        var svc = new FavoriteService(ctx, cache);

        var removed = await svc.RemoveAsync("user1", movie.Id);

        Assert.True(removed);
        Assert.False(cache.TryGetValue(key, out _));
    }

    [Fact]
    public async Task RemoveAsync_NonExistingFavorite_ReturnsFalse()
    {
        var dbName = nameof(RemoveAsync_NonExistingFavorite_ReturnsFalse);
        using var ctx = TestHelpers.CreateContext(dbName);

        var cache = TestHelpers.CreateMemoryCache();
        var svc = new FavoriteService(ctx, cache);

        var removed = await svc.RemoveAsync("user1", 9999);

        Assert.False(removed);
    }
}

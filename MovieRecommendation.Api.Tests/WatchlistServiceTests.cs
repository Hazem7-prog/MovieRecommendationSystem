using Microsoft.Extensions.Caching.Memory;
using MovieRecommendation.Api.DTOs.Watchlists;
using MovieRecommendation.Api.Models;
using MovieRecommendation.Api.Services;
using MovieRecommendation.Api.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class WatchlistServiceTests
{
    [Fact]
    public async Task AddAsync_WithValidWatchlist_Succeeds_AndInvalidatesCache()
    {
        var dbName =
            nameof(AddAsync_WithValidWatchlist_Succeeds_AndInvalidatesCache);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var movie =
            TestHelpers.CreateMovie("W1");

        ctx.Movies.Add(movie);

        await ctx.SaveChangesAsync();

        var cache =
            TestHelpers.CreateMemoryCache();

        var key =
            "recommendations_user1";

        cache.Set(
            key,
            new List<object> { 1 });

        var svc =
            new WatchlistService(ctx, cache);

        var dto =
            new AddWatchlistDto
            {
                MovieId = movie.Id
            };

        var result =
            await svc.AddAsync(
                "user1",
                dto);

        Assert.NotNull(result);

        Assert.Equal(
            movie.Id,
            result.Id);

        Assert.False(
            cache.TryGetValue(
                key,
                out _));
    }

    [Fact]
    public async Task AddAsync_DuplicateWatchlist_ThrowsInvalidOperationException()
    {
        var dbName =
            nameof(AddAsync_DuplicateWatchlist_ThrowsInvalidOperationException);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var movie =
            TestHelpers.CreateMovie("W1");

        ctx.Movies.Add(movie);

        ctx.Watchlists.Add(
            new Watchlist
            {
                Movie = movie,
                MovieId = movie.Id,
                UserId = "user1",
                AddedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new WatchlistService(
                ctx,
                cache);

        var dto =
            new AddWatchlistDto
            {
                MovieId = movie.Id
            };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AddAsync(
                "user1",
                dto));
    }

    [Fact]
    public async Task RemoveAsync_ExistingWatchlist_ReturnsTrue_AndInvalidatesCache()
    {
        var dbName =
            nameof(RemoveAsync_ExistingWatchlist_ReturnsTrue_AndInvalidatesCache);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var movie =
            TestHelpers.CreateMovie("W1");

        ctx.Movies.Add(movie);

        ctx.Watchlists.Add(
            new Watchlist
            {
                Movie = movie,
                MovieId = movie.Id,
                UserId = "user1",
                AddedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var cache =
            TestHelpers.CreateMemoryCache();

        var key =
            "recommendations_user1";

        cache.Set(
            key,
            new List<object> { 1 });

        var svc =
            new WatchlistService(
                ctx,
                cache);

        var removed =
            await svc.RemoveAsync(
                "user1",
                movie.Id);

        Assert.True(removed);

        Assert.False(
            cache.TryGetValue(
                key,
                out _));
    }

    [Fact]
    public async Task RemoveAsync_NonExistingWatchlist_ReturnsFalse()
    {
        var dbName =
            nameof(RemoveAsync_NonExistingWatchlist_ReturnsFalse);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new WatchlistService(
                ctx,
                cache);

        var removed =
            await svc.RemoveAsync(
                "user1",
                9999);

        Assert.False(removed);
    }
}
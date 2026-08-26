using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieRecommendation.Api.DTOs.AI;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Models;
using MovieRecommendation.Api.Services;
using MovieRecommendation.Api.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class RecommendationServiceTests
{
    [Fact]
    public async Task GetRecommendationsAsync_ColdStart_ReturnsDiscoveryOrderedByAverageRating()
    {
        var dbName =
            nameof(GetRecommendationsAsync_ColdStart_ReturnsDiscoveryOrderedByAverageRating);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var g1 =
            TestHelpers.CreateGenre("G1");

        var g2 =
            TestHelpers.CreateGenre("G2");

        var m1 =
            TestHelpers.CreateMovie(
                "M1",
                genres: new[] { g1 });

        var m2 =
            TestHelpers.CreateMovie(
                "M2",
                genres: new[] { g2 });

        var m3 =
            TestHelpers.CreateMovie(
                "M3",
                genres: new[] { g1 });

        ctx.Movies.AddRange(
            m1,
            m2,
            m3);

        await ctx.SaveChangesAsync();

        ctx.Ratings.Add(
            new Rating
            {
                MovieId = m1.Id,
                Movie = m1,
                UserId = "u1",
                Score = 9,
                CreatedAt = DateTime.UtcNow
            });

        ctx.Ratings.Add(
            new Rating
            {
                MovieId = m2.Id,
                Movie = m2,
                UserId = "u2",
                Score = 7,
                CreatedAt = DateTime.UtcNow
            });

        ctx.Ratings.Add(
            new Rating
            {
                MovieId = m3.Id,
                Movie = m3,
                UserId = "u3",
                Score = 8,
                CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var aiMock =
            new Mock<IAIService>();

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new RecommendationService(
                ctx,
                aiMock.Object,
                cache,
                new NullLogger<RecommendationService>());

        var recs =
            await svc.GetRecommendationsAsync(
                "newuser");

        Assert.NotEmpty(recs);

        Assert.True(
            recs.All(r =>
                r.RecommendationType == "Discovery"));

        var avgRatings =
            recs
                .Select(r => r.AverageRating)
                .ToList();

        var sorted =
            avgRatings
                .OrderByDescending(x => x)
                .ToList();

        Assert.Equal(
            sorted,
            avgRatings);

        Assert.True(
            recs.Count <= 10);
    }

    [Fact]
    public async Task GetRecommendationsAsync_Personalized_IncludesPersonalizedAndExcludesInteracted()
    {
        var dbName =
            nameof(GetRecommendationsAsync_Personalized_IncludesPersonalizedAndExcludesInteracted);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var gAction =
            TestHelpers.CreateGenre("Action");

        var gDrama =
            TestHelpers.CreateGenre("Drama");

        var likedMovie =
            TestHelpers.CreateMovie(
                "Liked",
                genres: new[] { gAction });

        var candidate1 =
            TestHelpers.CreateMovie(
                "Candidate1",
                genres: new[] { gAction });

        var candidate2 =
            TestHelpers.CreateMovie(
                "Candidate2",
                genres: new[] { gDrama });

        ctx.Movies.AddRange(
            likedMovie,
            candidate1,
            candidate2);

        await ctx.SaveChangesAsync();

        ctx.Ratings.Add(
            new Rating
            {
                Movie = likedMovie,
                MovieId = likedMovie.Id,
                UserId = "userX",
                Score = 8,
                CreatedAt = DateTime.UtcNow
            });

        ctx.Favorites.Add(
            new Favorite
            {
                Movie = candidate2,
                MovieId = candidate2.Id,
                UserId = "userX",
                CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var aiMock =
            new Mock<IAIService>();

        aiMock
            .Setup(a =>
                a.GenerateRecommendationExplanationsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<List<MovieForAIRequestDto>>()))
            .ReturnsAsync(
                new List<AIRecommendationExplanationDto>());

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new RecommendationService(
                ctx,
                aiMock.Object,
                cache,
                new NullLogger<RecommendationService>());

        var recs =
            await svc.GetRecommendationsAsync(
                "userX");

        Assert.DoesNotContain(
            recs,
            r => r.MovieId == likedMovie.Id);

        Assert.DoesNotContain(
            recs,
            r => r.MovieId == candidate2.Id);

        Assert.Contains(
            recs,
            r =>
                r.MovieId == candidate1.Id &&
                r.RecommendationType == "Personalized");
    }

    [Fact]
    public async Task GetRecommendationsAsync_CacheBehavior_UsesCacheWhenAvailable()
    {
        var dbName =
            nameof(GetRecommendationsAsync_CacheBehavior_UsesCacheWhenAvailable);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var movie =
            TestHelpers.CreateMovie("MC");

        ctx.Movies.Add(movie);

        await ctx.SaveChangesAsync();

        var aiMock =
            new Mock<IAIService>();

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new RecommendationService(
                ctx,
                aiMock.Object,
                cache,
                new NullLogger<RecommendationService>());

        var first =
            await svc.GetRecommendationsAsync(
                "u1");

        Assert.True(
            cache.TryGetValue(
                "recommendations_u1",
                out _));

        ctx.Movies.Add(
            TestHelpers.CreateMovie(
                "NewMovie"));

        await ctx.SaveChangesAsync();

        var second =
            await svc.GetRecommendationsAsync(
                "u1");

        Assert.Equal(
            first.Count,
            second.Count);
    }

    [Fact]
    public async Task GetRecommendationsAsync_AIServiceSuccess_UsesAIReasonAndConfidence()
    {
        var dbName =
            nameof(GetRecommendationsAsync_AIServiceSuccess_UsesAIReasonAndConfidence);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var genre =
            TestHelpers.CreateGenre("SciFi");

        var likedMovie =
            TestHelpers.CreateMovie(
                "LikedMovie",
                genres: new[] { genre });

        var candidate =
            TestHelpers.CreateMovie(
                "Candidate",
                genres: new[] { genre });

        ctx.Movies.AddRange(
            likedMovie,
            candidate);

        await ctx.SaveChangesAsync();

        ctx.Ratings.Add(
            new Rating
            {
                MovieId = likedMovie.Id,
                Movie = likedMovie,
                UserId = "user1",
                Score = 9,
                CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var aiMock =
            new Mock<IAIService>();

        aiMock
            .Setup(a =>
                a.GenerateRecommendationExplanationsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<List<MovieForAIRequestDto>>()))
            .ReturnsAsync(
                new List<AIRecommendationExplanationDto>
                {
                    new AIRecommendationExplanationDto
                    {
                        MovieId = candidate.Id,
                        Reason = "AI says watch",
                        Confidence = 0.9
                    }
                });

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new RecommendationService(
                ctx,
                aiMock.Object,
                cache,
                new NullLogger<RecommendationService>());

        var recs =
            await svc.GetRecommendationsAsync(
                "user1");

        var item =
            recs.FirstOrDefault(
                r => r.MovieId == candidate.Id);

        Assert.NotNull(item);

        Assert.Equal(
            "AI says watch",
            item!.Reason);

        Assert.Equal(
            0.9,
            item.Confidence,
            3);
    }

    [Fact]
    public async Task GetRecommendationsAsync_AIServiceThrows_UsesFallbackReasonAndZeroConfidence()
    {
        var dbName =
            nameof(GetRecommendationsAsync_AIServiceThrows_UsesFallbackReasonAndZeroConfidence);

        using var ctx =
            TestHelpers.CreateContext(dbName);

        var genre =
            TestHelpers.CreateGenre("Blend");

        var likedMovie =
            TestHelpers.CreateMovie(
                "LikedMovie",
                genres: new[] { genre });

        var candidate =
            TestHelpers.CreateMovie(
                "Candidate",
                genres: new[] { genre });

        ctx.Movies.AddRange(
            likedMovie,
            candidate);

        await ctx.SaveChangesAsync();

        ctx.Ratings.Add(
            new Rating
            {
                MovieId = likedMovie.Id,
                Movie = likedMovie,
                UserId = "user1",
                Score = 9,
                CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var aiMock =
            new Mock<IAIService>();

        aiMock
            .Setup(a =>
                a.GenerateRecommendationExplanationsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<List<MovieForAIRequestDto>>()))
            .ThrowsAsync(
                new Exception("AI down"));

        var cache =
            TestHelpers.CreateMemoryCache();

        var svc =
            new RecommendationService(
                ctx,
                aiMock.Object,
                cache,
                new NullLogger<RecommendationService>());

        var recs =
            await svc.GetRecommendationsAsync(
                "user1");

        var item =
            recs.FirstOrDefault(
                r => r.MovieId == candidate.Id);

        Assert.NotNull(item);

        Assert.False(
            string.IsNullOrWhiteSpace(
                item!.Reason));

        Assert.Equal(
            0,
            item.Confidence);
    }
}
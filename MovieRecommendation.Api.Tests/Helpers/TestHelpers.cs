using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Tests.Helpers;

public static class TestHelpers
{
    public static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var ctx = new ApplicationDbContext(options);

        return ctx;
    }

    public static IMemoryCache CreateMemoryCache() => new MemoryCache(new MemoryCacheOptions());

    public static NullLogger<T> CreateLogger<T>() => new NullLogger<T>();

    public static Movie CreateMovie(string title, int id = 0, double avgRating = 0, params Genre[] genres)
    {
        var movie = new Movie
        {
            Title = title,
            Description = "desc",
            Duration = 120,
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Language = "en",
            AgeRating = "PG",
            PosterUrl = "http://example.com/poster.jpg",
            Directors = "Dir",
            CastMembers = "Cast",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Genres = genres.ToList()
        };

        if (id > 0)
        {
            movie.Id = id;
        }

        return movie;
    }

    public static Genre CreateGenre(string name, int id = 0)
    {
        var g = new Genre { Name = name };
        if (id > 0) g.Id = id;
        return g;
    }
}

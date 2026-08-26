using System.ComponentModel.DataAnnotations;
using MovieRecommendation.Api.DTOs.Movies;
using MovieRecommendation.Api.DTOs.Ratings;
using Xunit;

namespace MovieRecommendation.Api.Tests;

public class ValidationTests
{
    [Fact]
    public void CreateRatingDto_InvalidScore_IsInvalid()
    {
        var dto = new CreateRatingDto { MovieId = 1, Score = 11 };

        var ctx = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(dto, ctx, results, true);

        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(dto.Score)));
    }

    [Fact]
    public void MovieQueryDto_PageSizeAboveMax_IsInvalid()
    {
        var dto = new MovieQueryDto { PageSize = 100 };

        var ctx = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(dto, ctx, results, true);

        Assert.False(valid);
    }

    [Fact]
    public void CreateMovieDto_MissingTitle_IsInvalid()
    {
        var dto = new CreateMovieDto { Title = string.Empty, ReleaseDate = default, GenreIds = new List<int>() };

        var ctx = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(dto, ctx, results, true);

        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(dto.Title)));
    }
}

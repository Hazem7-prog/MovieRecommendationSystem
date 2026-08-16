namespace MovieRecommendation.Api.DTOs.Movies;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}
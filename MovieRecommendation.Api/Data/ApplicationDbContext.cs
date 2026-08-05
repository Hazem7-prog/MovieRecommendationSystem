using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieRecommendation.Api.Models;

namespace MovieRecommendation.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }

    public DbSet<Genre> Genres { get; set; }

    public DbSet<Rating> Ratings { get; set; }

    public DbSet<Favorite> Favorites { get; set; }

    public DbSet<Watchlist> Watchlists { get; set; }
}
using Microsoft.AspNetCore.Identity;

namespace MovieRecommendation.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ICollection<Rating> Ratings { get; set; } = [];

        public ICollection<Favorite> Favorites { get; set; } = [];

        public ICollection<Watchlist> Watchlists { get; set; } = [];
    }
}

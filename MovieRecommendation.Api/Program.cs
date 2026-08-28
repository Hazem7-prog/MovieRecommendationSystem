using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using MovieRecommendation.Api.Data;
using MovieRecommendation.Api.Interfaces;
using MovieRecommendation.Api.Middleware;
using MovieRecommendation.Api.Models;
using MovieRecommendation.Api.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// External Only Mode
// ======================================================
// Local:
// false by default -> full API + Database + Identity.
//
// Deployment:
// ExternalOnlyMode=true
// -> External recommendation endpoint can run
//    without needing the database startup seed.
// ======================================================

var externalOnlyMode =
    builder.Configuration.GetValue<bool>(
        "ExternalOnlyMode");

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));
});

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtKey =
    builder.Configuration["JWT:Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["JWT:Issuer"],

            ValidAudience =
                builder.Configuration["JWT:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        jwtKey)),

            ClockSkew = TimeSpan.Zero,

            NameClaimType =
                ClaimTypes.Name,

            RoleClaimType =
                ClaimTypes.Role
        };
});

// Cache
builder.Services.AddMemoryCache();

// Dependency Injection
builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

builder.Services.AddScoped<
    IMovieService,
    MovieService>();

builder.Services.AddScoped<
    IGenreService,
    GenreService>();

builder.Services.AddScoped<
    IRatingService,
    RatingService>();

builder.Services.AddScoped<
    IFavoriteService,
    FavoriteService>();

builder.Services.AddScoped<
    IWatchlistService,
    WatchlistService>();

builder.Services.AddScoped<
    IRecommendationService,
    RecommendationService>();

builder.Services.AddScoped<
    ITmdbService,
    TmdbService>();

// TMDB
builder.Services.AddHttpClient("TMDB", client =>
{
    client.BaseAddress =
        new Uri(
            "https://api.themoviedb.org/3/");

    var token =
        builder.Configuration[
            "TMDB:AccessToken"];

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            token);
});

// Ollama AI
// External recommendations do NOT use Ollama.
builder.Services.AddHttpClient<
    IAIService,
    OllamaAIService>(client =>
    {
        client.BaseAddress =
            new Uri(
                builder.Configuration[
                    "Ollama:BaseUrl"]
                ?? "http://localhost:11434/");

        client.Timeout =
            TimeSpan.FromSeconds(30);
    });

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "Movie Recommendation API",

            Version =
                "v1"
        });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT",

            In =
                ParameterLocation.Header,

            Description =
                "Enter JWT Token"
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id =
                                "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(
        "recommendations",
        httpContext =>
        {
            var userId =
                httpContext.User
                    .FindFirst(
                        ClaimTypes.NameIdentifier)
                    ?.Value

                ?? httpContext.Connection
                    .RemoteIpAddress
                    ?.ToString()

                ?? "anonymous";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: userId,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0
                        });
        });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// ======================================================
// Seed Roles + Admin
// ======================================================
// Only run this when using the full application.
//
// In ExternalOnlyMode we intentionally skip database
// startup work because the external recommendation
// endpoint does not require our local database.
// ======================================================

if (!externalOnlyMode)
{
    using var scope =
        app.Services.CreateScope();

    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

    string[] roles =
    {
        "User",
        "Admin"
    };

    foreach (var role in roles)
    {
        if (!await roleManager
                .RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new IdentityRole(role));
        }
    }

    var adminEmail =
        builder.Configuration[
            "Admin:Email"];

    var adminPassword =
        builder.Configuration[
            "Admin:Password"];

    if (!string.IsNullOrWhiteSpace(
            adminEmail) &&
        !string.IsNullOrWhiteSpace(
            adminPassword))
    {
        var adminUser =
            await userManager
                .FindByEmailAsync(
                    adminEmail);

        if (adminUser == null)
        {
            adminUser =
                new ApplicationUser
                {
                    FullName =
                        "System Admin",

                    UserName =
                        adminEmail,

                    Email =
                        adminEmail,

                    CreatedAt =
                        DateTime.UtcNow
                };

            var result =
                await userManager
                    .CreateAsync(
                        adminUser,
                        adminPassword);

            if (result.Succeeded)
            {
                await userManager
                    .AddToRoleAsync(
                        adminUser,
                        "Admin");
            }
        }
    }
}

// Swagger available online for integration testing
app.UseSwagger();
app.UseSwaggerUI();

// Global Exception Handling
app.UseMiddleware<
    GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();
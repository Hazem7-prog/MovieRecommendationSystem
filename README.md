# AI-Powered Movie Recommendation API

A backend-focused movie recommendation system built with ASP.NET Core and enhanced with a local Large Language Model using Ollama.

The system learns user preferences from ratings, favorites, and watchlists, then generates personalized movie recommendations using a weighted recommendation algorithm.

AI is used to generate grounded explanations for recommendations, while the recommendation decision itself remains controlled by backend business logic.

---

## Features

### Authentication & Authorization

* User registration and login
* ASP.NET Core Identity
* JWT Authentication
* Role-based authorization
* User and Admin roles
* Admin-protected movie management endpoints

---

### Movie Management

* Create movies
* Update movies
* Soft delete movies
* Retrieve movie details
* Search movies
* Filter by genre
* Filter by language
* Filter by release year
* Filter by minimum rating
* Sorting
* Pagination

---

### Genres

* Create genres
* Retrieve genres
* Many-to-many relationship between Movies and Genres

---

### Ratings

Users can:

* Rate movies from 1 to 10
* View their ratings
* Rate each movie only once

Ratings contribute strongly to the recommendation algorithm.

---

### Favorites

Users can:

* Add movies to favorites
* Remove movies from favorites
* View favorite movies

Favorites contribute to user preference detection.

---

### Watchlist

Users can:

* Add movies to a watchlist
* Remove movies from the watchlist
* View their watchlist

Watchlist interactions also contribute to recommendations.

---

# Recommendation Engine

The recommendation engine uses weighted user interactions.

Current weights:

* High Rating (7+) = +3
* Favorite = +2
* Watchlist = +1

These interactions are used to calculate genre preference scores.

Candidate movies receive a recommendation score based on:

1. Matching genre preference score
2. Average movie rating

The highest-scoring movies are returned as recommendations.

Previously interacted movies are excluded from normal recommendation candidates.

---

## Personalized vs Discovery Recommendations

Recommendations are classified into two types.

### Personalized

A movie is marked as:

Personalized

when it matches one or more genres inferred from the user's interactions.

### Discovery

A movie can be marked as:

Discovery

when it falls outside the user's normal preferences.

This provides an exploration mechanism that allows users to discover new genres instead of receiving only predictable recommendations.

---

# Cold Start

New users have no ratings, favorites, or watchlist data.

This creates the classic recommendation-system problem known as:

Cold Start

For new users, the API returns discovery recommendations based primarily on globally highly rated movies.

These recommendations are returned without requiring AI processing.

---

# AI Integration

The application uses:

Ollama + Qwen3 4B

The AI model runs locally.

Default configuration:

"Ollama": {
"BaseUrl": "[http://localhost:11434/](http://localhost:11434/)",
"Model": "qwen3:4b"
}

Run the model using:

ollama run qwen3:4b

---

## AI Responsibilities

The AI does NOT decide which movies should be recommended.

The backend recommendation engine determines:

* Candidate movies
* Recommendation scores
* Recommendation ranking
* Personalized / Discovery classification

The AI is used only to generate human-readable explanations.

Architecture:

User Interactions
↓
Recommendation Engine
↓
Recommendation Score
↓
Top Movies
↓
Ollama
↓
Reason + Confidence

---

# AI Grounding

Recommendation explanations are grounded using verified backend data.

The AI receives information such as:

* Preferred genres
* High-rated movies
* Favorites
* Watchlist interactions
* Candidate movie genres

The prompt explicitly prevents the model from inventing user interactions.

This reduces hallucination and keeps explanations tied to actual system data.

---

# Structured AI Output

The AI returns structured JSON responses.

Example:

{
"movieId": 13,
"reason": "Matches the user's preference for Sci-Fi and Thriller.",
"confidence": 0.85
}

Structured output makes the response easier and safer for the backend to process.

---

# AI Batching

Instead of sending one AI request for every recommended movie, the application sends all recommendation candidates in a single request.

Before:

10 movies
→ 10 AI requests

After batching:

10 movies
→ 1 AI request
→ 10 explanations

This reduces latency and AI workload.

---

# Caching

The project uses ASP.NET Core IMemoryCache.

Recommendation results are cached per user.

Example cache key:

recommendations_{userId}

Normal recommendation results are cached temporarily to avoid recalculating recommendations and calling the AI repeatedly.

---

## Cache Invalidation

Recommendation cache entries are invalidated whenever user preference data changes.

The cache is cleared after:

* Adding a Rating
* Adding a Favorite
* Removing a Favorite
* Adding a Watchlist item
* Removing a Watchlist item

This prevents stale recommendation results.

---

# Rate Limiting

The recommendation endpoint is protected using ASP.NET Core Rate Limiting.

Current policy:

5 recommendation requests per minute per user

Requests exceeding the limit receive:

429 Too Many Requests

---

# AI Resilience & Fallback

AI availability does not determine whether the recommendation endpoint works.

If Ollama:

* Is offline
* Times out
* Returns invalid data
* Throws an exception

the backend generates recommendation explanations itself.

Example fallback:

Recommended based on your preference for Sci-Fi, Thriller.

Discovery fallback:

A discovery pick outside your usual genres, giving you something different to explore.

The recommendation endpoint therefore remains available even if the AI service fails.

---

# AI Timeout

AI requests have a timeout.

If Ollama takes too long to respond, the request is cancelled and backend fallback logic is used.

This prevents AI calls from blocking recommendation requests indefinitely.

---

# Logging & Observability

The application logs important recommendation events including:

* Recommendation request start
* Cache HIT / MISS
* Number of user interactions loaded
* Number of recommendation candidates
* AI request duration
* AI success or failure
* Fallback usage
* Total recommendation request duration

This makes it easier to diagnose performance and reliability problems.

---

# Global Error Handling

The API uses centralized exception handling middleware.

Exceptions are converted into consistent API responses.

Example:

{
"statusCode": 404,
"message": "Movie not found."
}

Examples:

* KeyNotFoundException → 404
* ArgumentException → 400
* InvalidOperationException → 409
* Unexpected exceptions → 500

---

# Validation

Request DTOs use built-in .NET Data Annotations.

Examples:

[Required]
[Range(1, 10)]
[MaxLength(200)]
[Url]

Validation includes:

* Movie rating range
* Required movie information
* Positive IDs
* Duration validation
* Pagination bounds
* URL validation
* Genre requirements

ASP.NET Core automatically returns HTTP 400 for invalid request models.

---

# Automated Testing

The project includes an xUnit test project:

MovieRecommendation.Api.Tests

Testing tools:

* xUnit
* Moq
* EF Core InMemory
* ASP.NET Core MemoryCache

The tests cover:

* Rating creation
* Duplicate ratings
* Favorite operations
* Watchlist operations
* Cache invalidation
* Cold Start
* Personalized recommendations
* Excluding previously interacted movies
* Recommendation caching
* AI success
* AI fallback
* DTO validation

Current test result:

Total: 19
Passed: 19
Failed: 0
Skipped: 0

Run tests using:

dotnet test

---

# Architecture

The project follows a simplified Layered / N-Tier Architecture.

Controllers
↓
Interfaces
↓
Services
↓
Entity Framework Core
↓
SQL Server

Additional layers include:

DTOs
Models
Middleware
Validation
AI Integration
Caching

AI integration follows:

RecommendationService
↓
IAIService
↓
OllamaAIService
↓
Ollama / Qwen3

---

# Technology Stack

Backend:

* C#
* ASP.NET Core Web API
* .NET 9

Database:

* SQL Server
* Entity Framework Core

Authentication:

* ASP.NET Core Identity
* JWT

AI:

* Ollama
* Qwen3 4B
* Structured JSON Output

Performance & Reliability:

* IMemoryCache
* Rate Limiting
* AI Batching
* Timeout Handling
* Backend Fallback

Testing:

* xUnit
* Moq
* EF Core InMemory

API Documentation:

* Swagger / OpenAPI

---

# Running the Project

## 1. Clone the repository

git clone <repository-url>

## 2. Restore packages

dotnet restore

## 3. Configure the database

Update the SQL Server connection string in configuration.

Example:

"ConnectionStrings": {
"DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
}

## 4. Apply migrations

dotnet ef database update

## 5. Install Ollama

Install Ollama on the machine.

## 6. Download the AI model

ollama pull qwen3:4b

## 7. Run Ollama

ollama run qwen3:4b

## 8. Run the API

dotnet run --project MovieRecommendation.Api

## 9. Run automated tests

dotnet test

---

# Main API Modules

/api/Auth
/api/Movies
/api/Genres
/api/Ratings
/api/Favorites
/api/Watchlists
/api/Recommendations

Swagger can be used in development to explore and test the API.

---

# Project Goal

The main goal of this project is to demonstrate how traditional backend engineering can be combined with practical AI capabilities.

The AI model is treated as an external intelligent component rather than replacing backend business logic.

This allows the system to remain:

* Deterministic where necessary
* Testable
* Resilient
* Maintainable
* Explainable
* AI-enhanced without being AI-dependent


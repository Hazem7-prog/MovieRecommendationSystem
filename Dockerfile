FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY MovieRecommendation.Api/MovieRecommendation.Api.csproj MovieRecommendation.Api/

RUN dotnet restore MovieRecommendation.Api/MovieRecommendation.Api.csproj

COPY . .

WORKDIR /src/MovieRecommendation.Api

RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MovieRecommendation.Api.dll"]
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY BattleSnakeStarter.sln .
COPY Starter.Api/Starter.Api.csproj Starter.Api/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY Starter.Api/ Starter.Api/

# Build and publish
WORKDIR /src/Starter.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Cloud Run sets the PORT environment variable
# ASP.NET Core will use this via ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:8080

# Expose port 8080 (Cloud Run default)
EXPOSE 8080

# Run as non-root user
USER $APP_UID

ENTRYPOINT ["dotnet", "Starter.Api.dll"]

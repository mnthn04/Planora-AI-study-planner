# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["AIStudyPlanner.sln", "./"]
COPY ["src/AIStudyPlanner.Domain/AIStudyPlanner.Domain.csproj", "src/AIStudyPlanner.Domain/"]
COPY ["src/AIStudyPlanner.Application/AIStudyPlanner.Application.csproj", "src/AIStudyPlanner.Application/"]
COPY ["src/AIStudyPlanner.Infrastructure/AIStudyPlanner.Infrastructure.csproj", "src/AIStudyPlanner.Infrastructure/"]
COPY ["src/AIStudyPlanner.API/AIStudyPlanner.API.csproj", "src/AIStudyPlanner.API/"]

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/src/AIStudyPlanner.API"
RUN dotnet publish "AIStudyPlanner.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "AIStudyPlanner.API.dll"]

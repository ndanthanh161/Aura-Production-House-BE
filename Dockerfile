# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Aura.API/Aura.API.csproj", "src/Aura.API/"]
COPY ["src/Aura.Application/Aura.Application.csproj", "src/Aura.Application/"]
COPY ["src/Aura.Domain/Aura.Domain.csproj", "src/Aura.Domain/"]
COPY ["src/Aura.Infrastructure/Aura.Infrastructure.csproj", "src/Aura.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Aura.API/Aura.API.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/src/Aura.API"
RUN dotnet publish "Aura.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "Aura.API.dll"]

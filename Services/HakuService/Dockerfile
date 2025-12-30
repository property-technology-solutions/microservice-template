# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/HakuService.API/HakuService.API.csproj", "src/HakuService.API/"]
COPY ["src/HakuService.Application/HakuService.Application.csproj", "src/HakuService.Application/"]
COPY ["src/HakuService.Domain/HakuService.Domain.csproj", "src/HakuService.Domain/"]
COPY ["src/HakuService.Infrastructure/HakuService.Infrastructure.csproj", "src/HakuService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/HakuService.API/HakuService.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/HakuService.API"
RUN dotnet build "HakuService.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "HakuService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "HakuService.API.dll"]


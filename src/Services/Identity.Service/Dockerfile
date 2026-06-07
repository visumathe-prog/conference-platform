# Multi-stage build for production optimization

# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (cached layer)
COPY ["src/Services/Identity.Service/Identity.Service.csproj", "Services/Identity.Service/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/SharedKernel/SharedKernel.csproj", "BuildingBlocks/SharedKernel/"]
COPY ["src/BuildingBlocks/Observability/Observability.csproj", "BuildingBlocks/Observability/"]

RUN dotnet restore "Services/Identity.Service/Identity.Service.csproj"

# Copy all source code
COPY src/ .

# Build the application
WORKDIR "/src/Services/Identity.Service"
RUN dotnet build "Identity.Service.csproj" -c Release -o /app/build

# ============================================
# Stage 2: Publish
# ============================================
FROM build AS publish
RUN dotnet publish "Identity.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 3: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Conference.Identity.Service.dll"]

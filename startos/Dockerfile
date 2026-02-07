# WabiView Dockerfile for StartOS
# Multi-stage build for optimized image size

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY src/WabiView/WabiView.csproj ./WabiView/
RUN dotnet restore WabiView/WabiView.csproj

# Copy source code and build
COPY src/WabiView/ ./WabiView/
WORKDIR /src/WabiView
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install required packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    jq \
    yq \
    && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Copy scripts
COPY startos/scripts/ /scripts/
RUN chmod +x /scripts/*.sh

# Create data directory
RUN mkdir -p /data

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV WABIVIEW_PORT=8080
ENV WABIVIEW_DATA_PATH=/data
ENV DOTNET_EnableDiagnostics=0

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["/scripts/entrypoint.sh"]

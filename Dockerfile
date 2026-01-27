# Multi-stage Dockerfile for .NET 10 Bookstore Application
# Optimized for Azure Kubernetes Service (AKS)

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Bookstore.NET/src/Bookstore.Web/Bookstore.Web.csproj", "Bookstore.Web/"]
RUN dotnet restore "Bookstore.Web/Bookstore.Web.csproj"

# Copy remaining source code
COPY Bookstore.NET/src/Bookstore.Web/ Bookstore.Web/

# Build and publish
WORKDIR "/src/Bookstore.Web"
RUN dotnet build "Bookstore.Web.csproj" -c Release -o /app/build
RUN dotnet publish "Bookstore.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from build stage
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Bookstore.Web.dll"]

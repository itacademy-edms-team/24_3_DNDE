# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files for better layer caching
COPY ["Directory.Packages.props", "Directory.Build.props", "nuget.config", "global.json", "./"]

# Copy only necessary project files
COPY ["src/FinanceTrack.Finance.Web/FinanceTrack.Finance.Web.csproj", "src/FinanceTrack.Finance.Web/"]
COPY ["src/FinanceTrack.Finance.Infrastructure/FinanceTrack.Finance.Infrastructure.csproj", "src/FinanceTrack.Finance.Infrastructure/"]
COPY ["src/FinanceTrack.Finance.Core/FinanceTrack.Finance.Core.csproj", "src/FinanceTrack.Finance.Core/"]
COPY ["src/FinanceTrack.Finance.UseCases/FinanceTrack.Finance.UseCases.csproj", "src/FinanceTrack.Finance.UseCases/"]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "./src/FinanceTrack.Finance.Web/FinanceTrack.Finance.Web.csproj"

# Copy source code (only what's needed - tests/docs excluded by .dockerignore)
COPY ["src/", "src/"]

WORKDIR "/src/src/FinanceTrack.Finance.Web"
RUN dotnet build "./FinanceTrack.Finance.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FinanceTrack.Finance.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceTrack.Finance.Web.dll"]
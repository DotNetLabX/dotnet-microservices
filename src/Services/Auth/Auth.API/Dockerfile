# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
ARG APP_NAME=Auth.API
ARG PROJECT_PATH=Services/Auth/${APP_NAME}

# Set working directory to project folder
WORKDIR /src/${PROJECT_PATH}

# Copy only the .csproj for caching restore
COPY ${PROJECT_PATH}/${APP_NAME}.csproj ./
RUN dotnet restore

# Copy ONLY the full project folder to prevent cross-service contamination
COPY ${PROJECT_PATH}/ ./

# Publish
RUN dotnet publish ${APP_NAME}.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set runtime app name (optional for logging, inspection, etc.)
ARG APP_NAME
ENV APP_NAME=${APP_NAME}

ENTRYPOINT ["dotnet", "/app/${APP_NAME}.dll"]

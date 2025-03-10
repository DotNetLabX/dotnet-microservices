FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
# IMPORTANT! This is how you run it:
# docker build -t auth-api-image --build-arg APP_NAME=Auth.API --build-arg PROJECT_PATH=Services/Auth/Auth.API .
ARG APP_NAME
ARG PROJECT_PATH

WORKDIR /src/${PROJECT_PATH}
COPY ${PROJECT_PATH}/${APP_NAME}.csproj ${PROJECT_PATH}/
RUN dotnet restore ${PROJECT_PATH}

# Copy the rest of the files after restoring dependencies
COPY . .

# Publish the app - there is no need for "dontnet build" since "dotnet publish" will do that
RUN dotnet publish ${PROJECT_PATH}/${APP_NAME}.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final Image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# We need to define the ARG again here in order to create an ENV variable for the runtime
ARG APP_NAME
ENV APP_NAME=${APP_NAME}
ENTRYPOINT dotnet /app/${APP_NAME}.dll

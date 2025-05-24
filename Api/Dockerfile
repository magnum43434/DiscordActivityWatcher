FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

RUN apt-get update && apt-get install -y curl

# Switch to root so we have permission to create and change the directory
USER root

# Create a new non-root user named "appuser"
# The command below uses "useradd". Depending on your base image,
# you might need slightly different syntax.
RUN useradd -ms /bin/bash appuser

# Create a directory for your SQLite DB (or other persistent data)
RUN mkdir -p /app/db && \
    chown -R appuser:appuser /app/db && \
    chmod 755 /app/db

# Optionally adjust ownership for your application files
COPY --chown=appuser:appuser . /app

# Switch to the non-root user for running your app
USER appuser

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]

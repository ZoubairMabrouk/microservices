# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj
COPY ["ConvesrsationHistoryService.csproj", "./"]

# Restore dependencies
RUN dotnet restore "ConvesrsationHistoryService.csproj"

# Copy everything
COPY . .

# Publish app
RUN dotnet publish "ConvesrsationHistoryService.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConvesrsationHistoryService.dll"]
# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first and restore — this layer caches unless deps change
COPY LendingApi.sln .
COPY src/LendingApi/LendingApi.csproj src/LendingApi/
RUN dotnet restore src/LendingApi/LendingApi.csproj

# Copy the rest of the source and publish
COPY src/ src/
RUN dotnet publish src/LendingApi/LendingApi.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "LendingApi.dll"]
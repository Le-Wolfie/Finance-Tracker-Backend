FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FinancialTracker.API.csproj", "./"]
RUN dotnet restore "FinancialTracker.API.csproj"

COPY . .
RUN dotnet publish "FinancialTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT at runtime; fallback to 10000 for local container runs.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet FinancialTracker.API.dll"]

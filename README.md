# FinancialTracker.API

ASP.NET Core backend for the Finance Tracker app.

## Requirements

- .NET SDK 9+
- MySQL 8+

## Environment

Create a `.env` file in this folder using `.env.example` as a starting point.

Required keys:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Jwt__ExpiryMinutes`

## Run locally

```bash
dotnet restore
dotnet run
```

## Build

```bash
dotnet build
```

## API docs

Swagger is enabled in development mode.

Default URL while running locally:

- `http://localhost:5104/swagger`

## Notes

- CORS origins are configured in `appsettings.Development.json`.

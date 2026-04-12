# FinancialTracker.API

Backend API for the Finance Tracker app.

## What This API Handles

- Authentication (register/login with JWT)
- Accounts management
- Categories management (income/expense)
- Transactions (create, read, update, delete)
- Budgets (monthly budgets, templates, rollover)
- Savings goals and contributions
- Recurring transaction rules and execution history
- Reporting endpoints (monthly summary, category breakdown, dashboard data)

## Feature Modules

Feature folders under `Features/`:

- `Auth`
- `Accounts`
- `Categories`
- `Transactions`
- `Budgets`
- `SavingsGoals`
- `RecurringTransactions`
- `Reporting`

## Tech Stack

- .NET 9 / ASP.NET Core Web API
- Entity Framework Core
- MySQL (Pomelo provider)
- FluentValidation
- JWT authentication
- Serilog logging
- Swagger/OpenAPI
- DotNetEnv (`.env` loading)

## Prerequisites

- .NET SDK 9+
- MySQL 8+

## Environment Setup

Create `.env` in this folder from `.env.example`.

Required values:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `Jwt__ExpiryMinutes`

Example:

```env
ConnectionStrings__DefaultConnection=server=localhost;port=3306;database=financial_tracker;user=root;password=your_password
Jwt__Issuer=FinancialTracker.API
Jwt__Audience=FinancialTracker.Client
Jwt__Key=replace_with_a_long_secure_key
Jwt__ExpiryMinutes=120
```

## Run Locally

From this folder:

```bash
dotnet restore
dotnet run
```

Default local API URL:

- `http://localhost:5104`

Swagger in development:

- `http://localhost:5104/swagger`

## Build and Checks

```bash
dotnet build
```

## Project Structure (Backend)

```text
FinancialTracker.API/
	Data/                 # EF Core DbContext and data setup
	DTOs/                 # Request/response models
	Entities/             # Domain entities
	Extensions/           # Service registration/auth/swagger extensions
	Features/             # Feature modules by domain
	Middleware/           # Global error handling, request pipeline helpers
	Migrations/           # EF Core migrations
	Services/             # Cross-feature shared services
	scripts/              # Smoke tests and seed utilities
	Program.cs            # API bootstrap and middleware setup
```

## Notes

- CORS origins are configured in `appsettings.Development.json`.
- `.env` values are loaded via DotNetEnv in `Program.cs`.

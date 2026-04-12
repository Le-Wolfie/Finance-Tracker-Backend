using DotNetEnv;
using FinancialTracker.API.Data;
using FinancialTracker.API.Extensions;
using FinancialTracker.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

// In container hosts, .env may not exist; runtime env vars are used instead.
try
{
    Env.Load();
}
catch
{
    // Ignore missing/invalid .env and continue with host-provided environment variables.
}

MapEnvIfMissing("CONNECTIONSTRINGS_DEFAULTCONNECTION", "ConnectionStrings__DefaultConnection");
MapEnvIfMissing("JWT_ISSUER", "Jwt__Issuer");
MapEnvIfMissing("JWT_AUDIENCE", "Jwt__Audience");
MapEnvIfMissing("JWT_KEY", "Jwt__Key");
MapEnvIfMissing("JWT_EXPIRYMINUTES", "Jwt__ExpiryMinutes");
MapEnvIfMissing("CORS_ALLOWEDORIGINS_0", "Cors__AllowedOrigins__0");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});
//^Serilog configuration is read from appsettings.json
// Serilog is a structured logging library that allows for more detailed and queryable logs compared to the default .NET logging. 

builder.Services.AddControllers(); // Adds support for controllers and API endpoints
builder.Services.AddEndpointsApiExplorer(); // Adds support for API endpoint discovery, which is used by Swagger to generate documentation
builder.Services.AddSwaggerWithJwt(); // Adds Swagger services and configures it to support JWT authentication
builder.Services.AddApplicationServices(); // Registers application services (e.g., IAuthService, IAccountsService) with the dependency injection container
builder.Services.AddJwtAuthentication(builder.Configuration); // Configures JWT authentication using settings from appsettings.json

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36)),
        mySql =>
        {
            mySql.EnableRetryOnFailure(3);
        });
});
//^ set up DB connection using MySQL. 
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>(); // Global error handling middleware to catch exceptions and return consistent error responses
app.UseSerilogRequestLogging(); // Middleware that logs HTTP requests and responses using Serilog
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "FinancialTracker.API",
    utc = DateTime.UtcNow
}));
app.MapControllers();

app.Run();

static void MapEnvIfMissing(string sourceKey, string targetKey)
{
    var sourceValue = Environment.GetEnvironmentVariable(sourceKey);
    var targetValue = Environment.GetEnvironmentVariable(targetKey);

    if (!string.IsNullOrWhiteSpace(sourceValue) && string.IsNullOrWhiteSpace(targetValue))
    {
        Environment.SetEnvironmentVariable(targetKey, sourceValue);
    }
}

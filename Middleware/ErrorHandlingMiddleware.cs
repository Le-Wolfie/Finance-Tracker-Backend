using System.Net;
using FinancialTracker.API.DTOs;
using FinancialTracker.API.Services;

namespace FinancialTracker.API.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Global error handling middleware that catches exceptions thrown during request processing and returns 
    /// consistent JSON error responses with appropriate HTTP status codes.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps exceptions to HTTP status codes and returns a JSON response containing error details, including a trace ID for correlation.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var rootException = ex is AggregateException aggregateException
            ? aggregateException.GetBaseException()
            : ex.GetBaseException();

        var (statusCode, title, code) = rootException switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "UNAUTHORIZED"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden", "FORBIDDEN"),
            NotFoundException => (HttpStatusCode.NotFound, "Not Found", "NOT_FOUND"),
            BusinessRuleException => (HttpStatusCode.BadRequest, "Business Rule Violation", "BUSINESS_RULE_VIOLATION"),
            _ => (HttpStatusCode.InternalServerError, "Server Error", "UNEXPECTED_ERROR")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponseDto
        {
            Code = code,
            Title = title,
            Status = (int)statusCode,
            Detail = rootException.Message,
            TraceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

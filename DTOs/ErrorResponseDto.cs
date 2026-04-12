namespace FinancialTracker.API.DTOs;

public sealed class ErrorResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}

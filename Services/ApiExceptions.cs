namespace FinancialTracker.API.Services;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

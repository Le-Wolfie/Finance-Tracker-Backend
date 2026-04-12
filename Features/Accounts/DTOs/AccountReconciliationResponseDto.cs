namespace FinancialTracker.API.Features.Accounts.DTOs;

public sealed class AccountReconciliationResponseDto
{
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal StoredBalance { get; set; }
    public decimal ComputedBalance { get; set; }
    public decimal Difference { get; set; }
    public bool IsMismatch { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalTransfersOut { get; set; }
    public decimal TotalTransfersIn { get; set; }
}

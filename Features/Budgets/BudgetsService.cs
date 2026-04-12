using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.Budgets.DTOs;
using FinancialTracker.API.Services;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.Budgets;

public sealed class BudgetsService : IBudgetsService
{
    private readonly AppDbContext _dbContext;

    public BudgetsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BudgetStatusResponseDto> SetAsync(SetBudgetRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.Type != CategoryType.Expense)
        {
            throw new BusinessRuleException("Budgets can only be set for expense categories.");
        }

        var budget = await _dbContext.Budgets.FirstOrDefaultAsync(
            x => x.UserId == userId && x.CategoryId == request.CategoryId && x.Year == request.Year && x.Month == request.Month,
            cancellationToken);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = userId,
                CategoryId = request.CategoryId,
                Year = request.Year,
                Month = request.Month,
                MonthlyLimit = request.MonthlyLimit
            };

            _dbContext.Budgets.Add(budget);
        }
        else
        {
            budget.MonthlyLimit = request.MonthlyLimit;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await BuildStatusAsync(budget, userId, cancellationToken);
    }

    public async Task<BudgetStatusResponseDto> GetStatusAsync(Guid categoryId, int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var budget = await _dbContext.Budgets.FirstOrDefaultAsync(
            x => x.UserId == userId && x.CategoryId == categoryId && x.Year == year && x.Month == month,
            cancellationToken) ?? throw new NotFoundException("Budget not found.");

        return await BuildStatusAsync(budget, userId, cancellationToken);
    }

    public async Task<IReadOnlyList<BudgetAlertDto>> GetAlertsAsync(int year, int month, decimal thresholdPercent, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var normalizedThreshold = Math.Clamp(thresholdPercent, 0, 100);

        var budgets = await _dbContext.Budgets
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Year == year && x.Month == month)
            .Select(x => new
            {
                x.Id,
                x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : "Uncategorized",
                x.MonthlyLimit,
                x.Year,
                x.Month
            })
            .ToListAsync(cancellationToken);

        if (budgets.Count == 0)
        {
            return [];
        }

        var categoryIds = budgets.Select(x => x.CategoryId).ToList();
        var spendingByCategory = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && x.Type == TransactionType.Expense
                && x.CategoryId.HasValue
                && categoryIds.Contains(x.CategoryId.Value)
                && x.Date >= start
                && x.Date < end)
            .GroupBy(x => x.CategoryId!.Value)
            .Select(x => new { CategoryId = x.Key, Spent = x.Sum(y => y.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent, cancellationToken);

        var alerts = budgets
            .Select(x =>
            {
                var spent = spendingByCategory.GetValueOrDefault(x.CategoryId, 0m);
                var usagePercent = x.MonthlyLimit <= 0 ? 0 : (spent / x.MonthlyLimit) * 100;
                var remaining = x.MonthlyLimit - spent;
                return new BudgetAlertDto
                {
                    BudgetId = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    Year = x.Year,
                    Month = x.Month,
                    MonthlyLimit = x.MonthlyLimit,
                    Spent = spent,
                    Remaining = remaining,
                    UsagePercent = Math.Round(usagePercent, 2),
                    IsExceeded = remaining < 0
                };
            })
            .Where(x => x.IsExceeded || x.UsagePercent >= normalizedThreshold)
            .OrderByDescending(x => x.UsagePercent)
            .ToList();

        return alerts;
    }

    public async Task<BudgetTemplateResponseDto> CreateTemplateAsync(CreateBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (category.Type != CategoryType.Expense)
        {
            throw new BusinessRuleException("Budget templates can only target expense categories.");
        }

        var exists = await _dbContext.BudgetTemplates
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Name == request.Name.Trim(), cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException("A template with the same name already exists.");
        }

        var template = new BudgetTemplate
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            MonthlyLimit = request.MonthlyLimit,
            RolloverStrategy = request.RolloverStrategy,
            IsActive = true
        };

        _dbContext.BudgetTemplates.Add(template);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToTemplateResponse(template);
    }

    public async Task<IReadOnlyList<BudgetTemplateResponseDto>> GetTemplatesAsync(BudgetTemplateFilterDto filter, Guid userId, CancellationToken cancellationToken)
    {
        var query = _dbContext.BudgetTemplates
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        var templates = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(ToTemplateResponse).ToList();
    }

    public async Task<BudgetTemplateResponseDto> UpdateTemplateAsync(Guid id, UpdateBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var template = await _dbContext.BudgetTemplates
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Budget template not found.");

        var duplicateName = await _dbContext.BudgetTemplates
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.Id != id && x.Name == request.Name.Trim(), cancellationToken);

        if (duplicateName)
        {
            throw new BusinessRuleException("A template with the same name already exists.");
        }

        template.Name = request.Name.Trim();
        template.Description = request.Description.Trim();
        template.MonthlyLimit = request.MonthlyLimit;
        template.RolloverStrategy = request.RolloverStrategy;
        template.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToTemplateResponse(template);
    }

    public async Task<BudgetStatusResponseDto> ApplyTemplateAsync(ApplyBudgetTemplateRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var template = await _dbContext.BudgetTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Budget template not found.");

        if (!template.IsActive)
        {
            throw new BusinessRuleException("Budget template is not active.");
        }

        var monthlyLimit = request.OverrideMonthlyLimit ?? template.MonthlyLimit;
        var budget = await _dbContext.Budgets
            .FirstOrDefaultAsync(x => x.UserId == userId
                && x.CategoryId == template.CategoryId
                && x.Year == request.Year
                && x.Month == request.Month, cancellationToken);

        if (budget is null)
        {
            budget = new Budget
            {
                UserId = userId,
                CategoryId = template.CategoryId,
                TemplateId = template.Id,
                Year = request.Year,
                Month = request.Month,
                MonthlyLimit = monthlyLimit
            };

            _dbContext.Budgets.Add(budget);
        }
        else
        {
            budget.TemplateId = template.Id;
            budget.MonthlyLimit = monthlyLimit;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await BuildStatusAsync(budget, userId, cancellationToken);
    }

    public async Task<BudgetRolloverResultDto> RunRolloverAsync(RunBudgetRolloverRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var (fromYear, fromMonth) = request.Month == 1
            ? (request.Year - 1, 12)
            : (request.Year, request.Month - 1);

        var previousBudgets = await _dbContext.Budgets
            .Include(x => x.Template)
            .Where(x => x.UserId == userId && x.Year == fromYear && x.Month == fromMonth)
            .ToListAsync(cancellationToken);

        if (previousBudgets.Count == 0)
        {
            return new BudgetRolloverResultDto
            {
                Year = request.Year,
                Month = request.Month,
                ProcessedCount = 0,
                SkippedCount = 0
            };
        }

        var fromStart = new DateTime(fromYear, fromMonth, 1);
        var fromEnd = fromStart.AddMonths(1);
        var categoryIds = previousBudgets.Select(x => x.CategoryId).Distinct().ToList();

        var previousSpentByCategory = await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && x.Type == TransactionType.Expense
                && x.CategoryId.HasValue
                && categoryIds.Contains(x.CategoryId.Value)
                && x.Date >= fromStart
                && x.Date < fromEnd)
            .GroupBy(x => x.CategoryId!.Value)
            .Select(x => new { CategoryId = x.Key, Spent = x.Sum(y => y.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent, cancellationToken);

        var processed = 0;
        var skipped = 0;

        foreach (var previousBudget in previousBudgets)
        {
            var existingRecord = await _dbContext.BudgetRolloverRecords
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId
                    && x.CategoryId == previousBudget.CategoryId
                    && x.ToYear == request.Year
                    && x.ToMonth == request.Month, cancellationToken);

            if (existingRecord)
            {
                skipped++;
                continue;
            }

            var previousSpent = previousSpentByCategory.GetValueOrDefault(previousBudget.CategoryId, 0m);
            var remaining = previousBudget.MonthlyLimit - previousSpent;
            var strategy = previousBudget.Template?.RolloverStrategy ?? BudgetRolloverStrategy.None;
            var rolledAmount = CalculateRolloverAmount(remaining, strategy);

            if (rolledAmount <= 0)
            {
                skipped++;
                continue;
            }

            var targetBudget = await _dbContext.Budgets
                .FirstOrDefaultAsync(x => x.UserId == userId
                    && x.CategoryId == previousBudget.CategoryId
                    && x.Year == request.Year
                    && x.Month == request.Month, cancellationToken);

            if (targetBudget is null)
            {
                targetBudget = new Budget
                {
                    UserId = userId,
                    CategoryId = previousBudget.CategoryId,
                    TemplateId = previousBudget.TemplateId,
                    RolloverFromBudgetId = previousBudget.Id,
                    Year = request.Year,
                    Month = request.Month,
                    MonthlyLimit = previousBudget.MonthlyLimit + rolledAmount
                };

                _dbContext.Budgets.Add(targetBudget);
            }
            else
            {
                targetBudget.MonthlyLimit += rolledAmount;
                targetBudget.RolloverFromBudgetId ??= previousBudget.Id;
            }

            var record = new BudgetRolloverRecord
            {
                UserId = userId,
                CategoryId = previousBudget.CategoryId,
                FromBudgetId = previousBudget.Id,
                ToBudget = targetBudget,
                FromYear = fromYear,
                FromMonth = fromMonth,
                ToYear = request.Year,
                ToMonth = request.Month,
                PreviousMonthlyLimit = previousBudget.MonthlyLimit,
                PreviousSpent = previousSpent,
                RolledOverAmount = rolledAmount,
                NewMonthlyLimit = targetBudget.MonthlyLimit,
                AppliedStrategy = strategy
            };

            _dbContext.BudgetRolloverRecords.Add(record);
            processed++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BudgetRolloverResultDto
        {
            Year = request.Year,
            Month = request.Month,
            ProcessedCount = processed,
            SkippedCount = skipped
        };
    }

    public async Task<IReadOnlyList<BudgetRolloverRecordResponseDto>> GetRolloverHistoryAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.BudgetRolloverRecords
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ToYear == year && x.ToMonth == month)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BudgetRolloverRecordResponseDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                FromBudgetId = x.FromBudgetId,
                ToBudgetId = x.ToBudgetId,
                FromYear = x.FromYear,
                FromMonth = x.FromMonth,
                ToYear = x.ToYear,
                ToMonth = x.ToMonth,
                PreviousMonthlyLimit = x.PreviousMonthlyLimit,
                PreviousSpent = x.PreviousSpent,
                RolledOverAmount = x.RolledOverAmount,
                NewMonthlyLimit = x.NewMonthlyLimit,
                AppliedStrategy = x.AppliedStrategy,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    private async Task<BudgetStatusResponseDto> BuildStatusAsync(Budget budget, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateTime(budget.Year, budget.Month, 1);
        var end = start.AddMonths(1);

        var spent = await _dbContext.Transactions
            .Where(x => x.UserId == userId
                && x.CategoryId == budget.CategoryId
                && x.Type == TransactionType.Expense
                && x.Date >= start
                && x.Date < end)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var remaining = budget.MonthlyLimit - spent;

        return new BudgetStatusResponseDto
        {
            BudgetId = budget.Id,
            CategoryId = budget.CategoryId,
            Year = budget.Year,
            Month = budget.Month,
            MonthlyLimit = budget.MonthlyLimit,
            Spent = spent,
            Remaining = remaining,
            IsExceeded = remaining < 0
        };
    }

    private static decimal CalculateRolloverAmount(decimal remaining, BudgetRolloverStrategy strategy)
    {
        if (remaining <= 0)
        {
            return 0m;
        }

        return strategy switch
        {
            BudgetRolloverStrategy.None => 0m,
            BudgetRolloverStrategy.UnusedOnly => remaining,
            BudgetRolloverStrategy.PartialUnused50Percent => remaining * 0.5m,
            _ => remaining
        };
    }

    private static BudgetTemplateResponseDto ToTemplateResponse(BudgetTemplate template)
    {
        return new BudgetTemplateResponseDto
        {
            Id = template.Id,
            CategoryId = template.CategoryId,
            Name = template.Name,
            Description = template.Description,
            MonthlyLimit = template.MonthlyLimit,
            RolloverStrategy = template.RolloverStrategy,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }
}

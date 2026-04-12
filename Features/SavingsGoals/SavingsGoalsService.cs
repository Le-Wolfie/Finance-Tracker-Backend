using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.SavingsGoals.DTOs;
using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.SavingsGoals;

public sealed class SavingsGoalsService : ISavingsGoalsService
{
    private readonly AppDbContext _dbContext;

    public SavingsGoalsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SavingsGoalResponseDto> CreateAsync(CreateSavingsGoalRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var accountExists = await _dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken);

        if (!accountExists)
        {
            throw new NotFoundException("Account not found.");
        }

        var goal = new SavingsGoal
        {
            UserId = userId,
            AccountId = request.AccountId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentAmount = 0m,
            StartDate = request.StartDate,
            TargetDate = request.TargetDate,
            Status = SavingsGoalStatus.Active
        };

        _dbContext.SavingsGoals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(goal);
    }

    public async Task<PagedResultDto<SavingsGoalResponseDto>> GetAsync(SavingsGoalFilterDto filter, Guid userId, CancellationToken cancellationToken)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

        var query = _dbContext.SavingsGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == filter.AccountId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.TargetDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SavingsGoalResponseDto>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SavingsGoalResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var goal = await _dbContext.SavingsGoals
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Savings goal not found.");

        return ToResponse(goal);
    }

    public async Task<SavingsGoalResponseDto> UpdateAsync(Guid id, UpdateSavingsGoalRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var goal = await _dbContext.SavingsGoals
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Savings goal not found.");

        goal.Name = request.Name.Trim();
        goal.Description = request.Description.Trim();
        goal.TargetAmount = request.TargetAmount;
        goal.TargetDate = request.TargetDate;
        goal.Status = request.Status;

        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.Status = SavingsGoalStatus.Completed;
            goal.CompletedDate ??= DateTime.UtcNow;
        }
        else if (goal.Status != SavingsGoalStatus.Completed)
        {
            goal.CompletedDate = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(goal);
    }

    public async Task<SavingsGoalContributionResponseDto> AddContributionAsync(Guid id, AddSavingsGoalContributionRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        var goal = await _dbContext.SavingsGoals
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Savings goal not found.");

        if (goal.Status is SavingsGoalStatus.Archived)
        {
            throw new BusinessRuleException("Cannot add contributions to archived goals.");
        }

        if (request.TransactionId.HasValue)
        {
            var txExists = await _dbContext.Transactions
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.TransactionId.Value && x.UserId == userId, cancellationToken);

            if (!txExists)
            {
                throw new NotFoundException("Transaction not found.");
            }
        }

        var contribution = new SavingsGoalContribution
        {
            SavingsGoalId = goal.Id,
            TransactionId = request.TransactionId,
            Amount = request.Amount,
            ContributionDate = request.ContributionDate,
            Note = request.Note.Trim()
        };

        goal.CurrentAmount += request.Amount;
        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.Status = SavingsGoalStatus.Completed;
            goal.CompletedDate ??= DateTime.UtcNow;
        }

        _dbContext.SavingsGoalContributions.Add(contribution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToContributionResponse(contribution);
    }

    public async Task<PagedResultDto<SavingsGoalContributionResponseDto>> GetContributionsAsync(Guid id, int page, int pageSize, Guid userId, CancellationToken cancellationToken)
    {
        var goalExists = await _dbContext.SavingsGoals
            .AsNoTracking()
            .AnyAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (!goalExists)
        {
            throw new NotFoundException("Savings goal not found.");
        }

        var resolvedPage = page <= 0 ? 1 : page;
        var resolvedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.SavingsGoalContributions
            .AsNoTracking()
            .Where(x => x.SavingsGoalId == id);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ContributionDate)
            .Skip((resolvedPage - 1) * resolvedPageSize)
            .Take(resolvedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SavingsGoalContributionResponseDto>
        {
            Items = items.Select(ToContributionResponse).ToList(),
            Page = resolvedPage,
            PageSize = resolvedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SavingsGoalResponseDto> MarkCompleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var goal = await _dbContext.SavingsGoals
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Savings goal not found.");

        goal.Status = SavingsGoalStatus.Completed;
        goal.CompletedDate ??= DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(goal);
    }

    public async Task<SavingsGoalsSummaryDto> GetSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var goals = await _dbContext.SavingsGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var totalTarget = goals.Sum(x => x.TargetAmount);
        var totalCurrent = goals.Sum(x => x.CurrentAmount);

        return new SavingsGoalsSummaryDto
        {
            ActiveGoalsCount = goals.Count(x => x.Status == SavingsGoalStatus.Active),
            CompletedGoalsCount = goals.Count(x => x.Status == SavingsGoalStatus.Completed),
            TotalTargetAmount = totalTarget,
            TotalCurrentAmount = totalCurrent,
            OverallCompletionPercent = totalTarget <= 0 ? 0 : Math.Round((totalCurrent / totalTarget) * 100m, 2)
        };
    }

    private static SavingsGoalResponseDto ToResponse(SavingsGoal goal)
    {
        var remaining = goal.TargetAmount - goal.CurrentAmount;
        var completion = goal.TargetAmount <= 0 ? 0 : Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100m, 2);

        return new SavingsGoalResponseDto
        {
            Id = goal.Id,
            AccountId = goal.AccountId,
            Name = goal.Name,
            Description = goal.Description,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            RemainingAmount = remaining,
            CompletionPercent = completion,
            StartDate = goal.StartDate,
            TargetDate = goal.TargetDate,
            CompletedDate = goal.CompletedDate,
            Status = goal.Status,
            CreatedAt = goal.CreatedAt
        };
    }

    private static SavingsGoalContributionResponseDto ToContributionResponse(SavingsGoalContribution contribution)
    {
        return new SavingsGoalContributionResponseDto
        {
            Id = contribution.Id,
            SavingsGoalId = contribution.SavingsGoalId,
            TransactionId = contribution.TransactionId,
            Amount = contribution.Amount,
            ContributionDate = contribution.ContributionDate,
            Note = contribution.Note,
            CreatedAt = contribution.CreatedAt
        };
    }
}

using FinancialTracker.API.Data;
using FinancialTracker.API.Entities;
using FinancialTracker.API.Features.RecurringTransactions.DTOs;
using FinancialTracker.API.Features.Transactions;
using FinancialTracker.API.Features.Transactions.DTOs;
using FinancialTracker.API.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Features.RecurringTransactions;

public sealed class RecurringTransactionsService : IRecurringTransactionsService
{
    private readonly AppDbContext _dbContext;
    private readonly ITransactionsService _transactionsService;

    public RecurringTransactionsService(AppDbContext dbContext, ITransactionsService transactionsService)
    {
        _dbContext = dbContext;
        _transactionsService = transactionsService;
    }

    public async Task<RecurringTransactionResponseDto> CreateAsync(CreateRecurringTransactionRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request.AccountId, request.DestinationAccountId, request.CategoryId, userId, cancellationToken);

        var recurring = new RecurringTransaction
        {
            UserId = userId,
            AccountId = request.AccountId,
            DestinationAccountId = request.DestinationAccountId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Frequency = request.Frequency,
            IntervalDays = request.IntervalDays,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MaxOccurrences = request.MaxOccurrences,
            NextExecutionDate = request.StartDate,
            IsActive = true
        };

        _dbContext.RecurringTransactions.Add(recurring);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return recurring.Adapt<RecurringTransactionResponseDto>();
    }

    public async Task<RecurringTransactionResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recurring = await _dbContext.RecurringTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        return recurring.Adapt<RecurringTransactionResponseDto>();
    }

    public async Task<PagedResultDto<RecurringTransactionResponseDto>> GetAsync(RecurringTransactionFilterDto filter, Guid userId, CancellationToken cancellationToken)
    {
        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

        var query = _dbContext.RecurringTransactions
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (filter.Frequency.HasValue)
        {
            query = query.Where(x => x.Frequency == filter.Frequency.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.NextExecutionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<RecurringTransactionResponseDto>()
            .ToListAsync(cancellationToken);

        return new PagedResultDto<RecurringTransactionResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RecurringTransactionResponseDto> UpdateAsync(Guid id, UpdateRecurringTransactionRequestDto request, Guid userId, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request.AccountId, request.DestinationAccountId, request.CategoryId, userId, cancellationToken);

        var recurring = await _dbContext.RecurringTransactions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        var scheduleChanged = recurring.StartDate != request.StartDate
            || recurring.Frequency != request.Frequency
            || recurring.IntervalDays != request.IntervalDays;

        recurring.AccountId = request.AccountId;
        recurring.DestinationAccountId = request.DestinationAccountId;
        recurring.CategoryId = request.CategoryId;
        recurring.Name = request.Name.Trim();
        recurring.Description = request.Description.Trim();
        recurring.Amount = request.Amount;
        recurring.Type = request.Type;
        recurring.Frequency = request.Frequency;
        recurring.IntervalDays = request.IntervalDays;
        recurring.StartDate = request.StartDate;
        recurring.EndDate = request.EndDate;
        recurring.MaxOccurrences = request.MaxOccurrences;

        if (request.IsActive && scheduleChanged)
        {
            recurring.NextExecutionDate = CalculateNextOccurrenceFromStartDate(
                request.StartDate,
                request.Frequency,
                request.IntervalDays,
                DateTime.UtcNow);
        }
        else
        {
            recurring.NextExecutionDate = request.NextExecutionDate;
        }

        recurring.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return recurring.Adapt<RecurringTransactionResponseDto>();
    }

    public async Task<RecurringTransactionResponseDto> PauseAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recurring = await _dbContext.RecurringTransactions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        recurring.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return recurring.Adapt<RecurringTransactionResponseDto>();
    }

    public async Task<RecurringTransactionResponseDto> ResumeAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recurring = await _dbContext.RecurringTransactions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        recurring.IsActive = true;
        if (recurring.NextExecutionDate < DateTime.UtcNow)
        {
            recurring.NextExecutionDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return recurring.Adapt<RecurringTransactionResponseDto>();
    }

    public async Task<RecurringTransactionExecutionResponseDto> ExecuteNowAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recurring = await _dbContext.RecurringTransactions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        var scheduledFor = DateTime.UtcNow;
        return await ExecuteSingleAsync(recurring, scheduledFor, cancellationToken);
    }

    public async Task<PagedResultDto<RecurringTransactionExecutionResponseDto>> GetExecutionsAsync(Guid recurringTransactionId, int page, int pageSize, Guid userId, CancellationToken cancellationToken)
    {
        var recurringExists = await _dbContext.RecurringTransactions
            .AsNoTracking()
            .AnyAsync(x => x.Id == recurringTransactionId && x.UserId == userId, cancellationToken);

        if (!recurringExists)
        {
            throw new NotFoundException("Recurring transaction not found.");
        }

        var resolvedPage = page <= 0 ? 1 : page;
        var resolvedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.RecurringTransactionExecutions
            .AsNoTracking()
            .Where(x => x.RecurringTransactionId == recurringTransactionId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ScheduledForDate)
            .Skip((resolvedPage - 1) * resolvedPageSize)
            .Take(resolvedPageSize)
            .ProjectToType<RecurringTransactionExecutionResponseDto>()
            .ToListAsync(cancellationToken);

        return new PagedResultDto<RecurringTransactionExecutionResponseDto>
        {
            Items = items,
            Page = resolvedPage,
            PageSize = resolvedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<int> ExecuteDueAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var due = await _dbContext.RecurringTransactions
            .Where(x => x.IsActive && x.NextExecutionDate <= now)
            .OrderBy(x => x.NextExecutionDate)
            .Take(100)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var recurring in due)
        {
            if (ShouldStopRecurring(recurring))
            {
                recurring.IsActive = false;
                continue;
            }

            await ExecuteSingleAsync(recurring, recurring.NextExecutionDate, cancellationToken);
            processed++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recurring = await _dbContext.RecurringTransactions
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Recurring transaction not found.");

        _dbContext.RecurringTransactions.Remove(recurring);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<RecurringTransactionExecutionResponseDto> ExecuteSingleAsync(RecurringTransaction recurring, DateTime scheduledFor, CancellationToken cancellationToken)
    {
        var existingExecution = await _dbContext.RecurringTransactionExecutions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RecurringTransactionId == recurring.Id && x.ScheduledForDate == scheduledFor, cancellationToken);

        if (existingExecution is not null)
        {
            return existingExecution.Adapt<RecurringTransactionExecutionResponseDto>();
        }

        var execution = new RecurringTransactionExecution
        {
            RecurringTransactionId = recurring.Id,
            ScheduledForDate = scheduledFor,
            ExecutedAt = DateTime.UtcNow,
            Status = RecurringExecutionStatus.Success
        };

        try
        {
            var createRequest = new CreateTransactionRequestDto
            {
                AccountId = recurring.AccountId,
                DestinationAccountId = recurring.DestinationAccountId,
                CategoryId = recurring.CategoryId,
                Amount = recurring.Amount,
                Type = recurring.Type,
                Date = scheduledFor,
                Description = recurring.Description
            };

            var idempotencyKey = $"recurring:{recurring.Id}:{scheduledFor:O}";
            var created = await _transactionsService.CreateAsync(createRequest, recurring.UserId, idempotencyKey, cancellationToken);

            execution.GeneratedTransactionId = created.Id;
            recurring.ExecutedCount += 1;
            recurring.NextExecutionDate = CalculateNextExecutionDate(recurring, scheduledFor);

            if (ShouldStopRecurring(recurring))
            {
                recurring.IsActive = false;
            }
        }
        catch (Exception ex)
        {
            execution.Status = RecurringExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
        }

        _dbContext.RecurringTransactionExecutions.Add(execution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return execution.Adapt<RecurringTransactionExecutionResponseDto>();
    }

    private static DateTime CalculateNextExecutionDate(RecurringTransaction recurring, DateTime fromDate)
    {
        return recurring.Frequency switch
        {
            RecurrenceFrequency.Daily => fromDate.AddDays(1),
            RecurrenceFrequency.Weekly => fromDate.AddDays(7),
            RecurrenceFrequency.Monthly => fromDate.AddMonths(1),
            RecurrenceFrequency.CustomIntervalDays => fromDate.AddDays(recurring.IntervalDays ?? 1),
            _ => fromDate.AddMonths(1)
        };
    }

    private static DateTime CalculateNextOccurrenceFromStartDate(
        DateTime startDate,
        RecurrenceFrequency frequency,
        int? intervalDays,
        DateTime now)
    {
        // If start date is in the future, return it as-is
        if (startDate > now)
        {
            return startDate;
        }

        // Start from the start date and calculate the next occurrence after "now"
        var next = startDate;
        var maxIterations = 1000; // Safety limit to prevent infinite loops
        var iterations = 0;

        while (next <= now && iterations < maxIterations)
        {
            next = frequency switch
            {
                RecurrenceFrequency.Daily => next.AddDays(1),
                RecurrenceFrequency.Weekly => next.AddDays(7),
                RecurrenceFrequency.Monthly => next.AddMonths(1),
                RecurrenceFrequency.CustomIntervalDays => next.AddDays(intervalDays ?? 1),
                _ => next.AddMonths(1)
            };
            iterations++;
        }

        return next;
    }

    private static bool ShouldStopRecurring(RecurringTransaction recurring)
    {
        if (recurring.MaxOccurrences.HasValue && recurring.ExecutedCount >= recurring.MaxOccurrences.Value)
        {
            return true;
        }

        if (recurring.EndDate.HasValue && recurring.NextExecutionDate > recurring.EndDate.Value)
        {
            return true;
        }

        return false;
    }

    private async Task ValidateReferencesAsync(Guid accountId, Guid? destinationAccountId, Guid? categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var sourceExists = await _dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(x => x.Id == accountId && x.UserId == userId, cancellationToken);

        if (!sourceExists)
        {
            throw new NotFoundException("Account not found.");
        }

        if (destinationAccountId.HasValue)
        {
            var destinationExists = await _dbContext.Accounts
                .AsNoTracking()
                .AnyAsync(x => x.Id == destinationAccountId.Value && x.UserId == userId, cancellationToken);

            if (!destinationExists)
            {
                throw new NotFoundException("Destination account not found.");
            }
        }

        if (categoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories
                .AsNoTracking()
                .AnyAsync(x => x.Id == categoryId.Value && x.UserId == userId, cancellationToken);

            if (!categoryExists)
            {
                throw new NotFoundException("Category not found.");
            }
        }
    }
}

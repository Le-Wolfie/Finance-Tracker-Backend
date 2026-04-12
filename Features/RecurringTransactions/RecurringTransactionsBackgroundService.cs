using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinancialTracker.API.Features.RecurringTransactions;

public sealed class RecurringTransactionsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringTransactionsBackgroundService> _logger;

    public RecurringTransactionsBackgroundService(IServiceProvider serviceProvider, ILogger<RecurringTransactionsBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var recurringService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionsService>();
                var processed = await recurringService.ExecuteDueAsync(stoppingToken);
                if (processed > 0)
                {
                    _logger.LogInformation("Processed {Processed} due recurring transactions.", processed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recurring transaction background job iteration failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}

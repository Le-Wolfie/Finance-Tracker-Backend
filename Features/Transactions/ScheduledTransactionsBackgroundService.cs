using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinancialTracker.API.Features.Transactions;

public sealed class ScheduledTransactionsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledTransactionsBackgroundService> _logger;

    public ScheduledTransactionsBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ScheduledTransactionsBackgroundService> logger)
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
                var transactionsService = scope.ServiceProvider.GetRequiredService<ITransactionsService>();
                var processed = await transactionsService.ApplyDueTransactionsAsync(stoppingToken);

                if (processed > 0)
                {
                    _logger.LogInformation("Applied {Processed} scheduled transactions.", processed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled transaction background job iteration failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

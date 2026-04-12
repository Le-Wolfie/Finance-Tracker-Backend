using System.Reflection;
using FinancialTracker.API.Features.Accounts;
using FinancialTracker.API.Features.Auth;
using FinancialTracker.API.Features.Budgets;
using FinancialTracker.API.Features.Categories;
using FinancialTracker.API.Features.RecurringTransactions;
using FinancialTracker.API.Features.Reporting;
using FinancialTracker.API.Features.SavingsGoals;
using FinancialTracker.API.Features.Transactions;
using FinancialTracker.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace FinancialTracker.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor(); // Registers the IHttpContextAccessor to allow services to access the current HTTP context, which is necessary for retrieving user information in the UserContextService
        services.AddScoped<IUserContextService, UserContextService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountsService, AccountsService>();
        services.AddScoped<ITransactionsService, TransactionsService>();
        services.AddScoped<IRecurringTransactionsService, RecurringTransactionsService>();
        services.AddScoped<ISavingsGoalsService, SavingsGoalsService>();
        services.AddScoped<ICategoriesService, CategoriesService>();
        services.AddScoped<IBudgetsService, BudgetsService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddHostedService<RecurringTransactionsBackgroundService>();

        services.AddFluentValidationAutoValidation(); // Enables automatic validation of incoming models using FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //^Registers all FluentValidation validators found in the current assembly, allowing them to be automatically applied to incoming request models without needing to manually specify each validator.

        return services;
    }
}

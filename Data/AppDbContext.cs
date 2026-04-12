using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancialTracker.API.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>(); // Equivalent to public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<RecurringTransactionExecution> RecurringTransactionExecutions => Set<RecurringTransactionExecution>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<SavingsGoalContribution> SavingsGoalContributions => Set<SavingsGoalContribution>();
    public DbSet<BudgetTemplate> BudgetTemplates => Set<BudgetTemplate>();
    public DbSet<BudgetRolloverRecord> BudgetRolloverRecords => Set<BudgetRolloverRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // Override SaveChangesAsync to automatically set CreatedAt and UpdatedAt when saving entities
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified);

        var utcNow = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = utcNow;
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTracker.API.Data.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MonthlyLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Template)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RolloverFromBudget)
            .WithMany(x => x.RolledOverToBudgets)
            .HasForeignKey(x => x.RolloverFromBudgetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.CategoryId, x.Year, x.Month })
            .IsUnique();

        builder.HasIndex(x => x.RolloverFromBudgetId);
    }
}

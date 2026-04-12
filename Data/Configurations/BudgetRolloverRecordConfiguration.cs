using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTracker.API.Data.Configurations;

public sealed class BudgetRolloverRecordConfiguration : IEntityTypeConfiguration<BudgetRolloverRecord>
{
    public void Configure(EntityTypeBuilder<BudgetRolloverRecord> builder)
    {
        builder.ToTable("BudgetRolloverRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PreviousMonthlyLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PreviousSpent)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.RolledOverAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.NewMonthlyLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.BudgetRolloverRecords)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.BudgetRolloverRecords)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FromBudget)
            .WithMany()
            .HasForeignKey(x => x.FromBudgetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ToBudget)
            .WithMany()
            .HasForeignKey(x => x.ToBudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.CategoryId, x.ToYear, x.ToMonth })
            .IsUnique();
    }
}

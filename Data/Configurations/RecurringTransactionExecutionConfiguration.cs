using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTracker.API.Data.Configurations;

public sealed class RecurringTransactionExecutionConfiguration : IEntityTypeConfiguration<RecurringTransactionExecution>
{
    public void Configure(EntityTypeBuilder<RecurringTransactionExecution> builder)
    {
        builder.ToTable("RecurringTransactionExecutions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(500);

        builder.HasOne(x => x.RecurringTransaction)
            .WithMany(x => x.Executions)
            .HasForeignKey(x => x.RecurringTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.GeneratedTransaction)
            .WithMany(x => x.RecurringExecutions)
            .HasForeignKey(x => x.GeneratedTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.RecurringTransactionId, x.ScheduledForDate })
            .IsUnique();
    }
}

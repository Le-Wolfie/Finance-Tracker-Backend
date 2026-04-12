using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTracker.API.Data.Configurations;

public sealed class SavingsGoalContributionConfiguration : IEntityTypeConfiguration<SavingsGoalContribution>
{
    public void Configure(EntityTypeBuilder<SavingsGoalContribution> builder)
    {
        builder.ToTable("SavingsGoalContributions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.SavingsGoal)
            .WithMany(x => x.Contributions)
            .HasForeignKey(x => x.SavingsGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Transaction)
            .WithMany(x => x.SavingsGoalContributions)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SavingsGoalId, x.ContributionDate });
    }
}

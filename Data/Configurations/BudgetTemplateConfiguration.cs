using FinancialTracker.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTracker.API.Data.Configurations;

public sealed class BudgetTemplateConfiguration : IEntityTypeConfiguration<BudgetTemplate>
{
    public void Configure(EntityTypeBuilder<BudgetTemplate> builder)
    {
        builder.ToTable("BudgetTemplates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.MonthlyLimit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.BudgetTemplates)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.BudgetTemplates)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.CategoryId, x.IsActive });
    }
}

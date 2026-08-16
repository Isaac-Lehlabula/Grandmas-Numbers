using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class DreamSymbolConfiguration : IEntityTypeConfiguration<DreamSymbol>
{
    public void Configure(EntityTypeBuilder<DreamSymbol> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.TraditionalMeaning).IsRequired().HasMaxLength(1000);

        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasMany(s => s.Aliases)
            .WithOne(a => a.DreamSymbol)
            .HasForeignKey(a => a.DreamSymbolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.LuckyNumbers)
            .WithOne(n => n.DreamSymbol)
            .HasForeignKey(n => n.DreamSymbolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

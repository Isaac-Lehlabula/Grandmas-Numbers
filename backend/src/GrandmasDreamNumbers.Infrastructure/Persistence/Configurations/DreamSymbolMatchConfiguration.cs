using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class DreamSymbolMatchConfiguration : IEntityTypeConfiguration<DreamSymbolMatch>
{
    public void Configure(EntityTypeBuilder<DreamSymbolMatch> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.MatchedText).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Explanation).IsRequired().HasMaxLength(1000);

        builder.HasOne(m => m.DreamSymbol)
            .WithMany()
            .HasForeignKey(m => m.DreamSymbolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

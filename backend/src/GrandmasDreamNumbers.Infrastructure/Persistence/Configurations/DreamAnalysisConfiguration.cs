using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class DreamAnalysisConfiguration : IEntityTypeConfiguration<DreamAnalysis>
{
    public void Configure(EntityTypeBuilder<DreamAnalysis> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.ModelUsed).IsRequired().HasMaxLength(100);

        builder.HasOne(a => a.Dream)
            .WithMany(d => d.Analyses)
            .HasForeignKey(a => a.DreamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Matches)
            .WithOne(m => m.DreamAnalysis)
            .HasForeignKey(m => m.DreamAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.SuggestedCombinations)
            .WithOne(c => c.DreamAnalysis)
            .HasForeignKey(c => c.DreamAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

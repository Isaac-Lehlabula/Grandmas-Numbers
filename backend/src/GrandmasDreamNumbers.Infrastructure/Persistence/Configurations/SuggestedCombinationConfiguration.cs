using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class SuggestedCombinationConfiguration : IEntityTypeConfiguration<SuggestedCombination>
{
    public void Configure(EntityTypeBuilder<SuggestedCombination> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CombinationType).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Numbers).IsRequired();
    }
}

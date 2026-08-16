using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class LuckyNumberConfiguration : IEntityTypeConfiguration<LuckyNumber>
{
    public void Configure(EntityTypeBuilder<LuckyNumber> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.Notes).HasMaxLength(500);
    }
}

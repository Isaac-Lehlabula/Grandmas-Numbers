using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class DreamConfiguration : IEntityTypeConfiguration<Dream>
{
    public void Configure(EntityTypeBuilder<Dream> builder)
    {
        // Id is always generated client-side (BaseEntity's property
        // initializer) - without this, EF's default Guid convention can
        // misjudge a new child added to an already-tracked parent's
        // collection as Modified instead of Added, producing a spurious
        // DbUpdateConcurrencyException. Applied to every entity here for
        // the same reason (found via live end-to-end testing).
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.OriginalText).IsRequired().HasMaxLength(4000);
        builder.Property(d => d.TranscribedText).HasMaxLength(4000);
        builder.Property(d => d.Summary).HasMaxLength(1000);

        builder.HasIndex(d => d.UserId);
    }
}

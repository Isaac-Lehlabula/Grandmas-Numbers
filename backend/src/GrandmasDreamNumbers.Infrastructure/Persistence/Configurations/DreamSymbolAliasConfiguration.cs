using GrandmasDreamNumbers.Domain.Dreams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class DreamSymbolAliasConfiguration : IEntityTypeConfiguration<DreamSymbolAlias>
{
    public void Configure(EntityTypeBuilder<DreamSymbolAlias> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Alias).IsRequired().HasMaxLength(100);

        builder.HasIndex(a => a.Alias);
    }
}

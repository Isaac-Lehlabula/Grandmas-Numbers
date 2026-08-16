using GrandmasDreamNumbers.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrandmasDreamNumbers.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // See DreamConfiguration.Id for why this is required.
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Token).IsRequired().HasMaxLength(200);

        builder.HasIndex(t => t.Token).IsUnique();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

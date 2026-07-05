using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(512);
        builder.Property(r => r.RevokedReason).HasMaxLength(200);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(512);

        builder.HasIndex(r => r.Token).IsUnique();
        builder.HasIndex(r => r.UserId);
    }
}

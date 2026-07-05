using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Type).HasConversion<string>();

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
    }
}

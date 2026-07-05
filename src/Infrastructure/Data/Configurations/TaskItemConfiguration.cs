using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.Priority).HasConversion<string>();
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.AttachmentPath).HasMaxLength(500);
        builder.Property(t => t.AttachmentFileName).HasMaxLength(255);

        builder.HasIndex(t => t.AssignedToId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueDate);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Notifications)
            .WithOne(n => n.Task)
            .HasForeignKey(n => n.TaskId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

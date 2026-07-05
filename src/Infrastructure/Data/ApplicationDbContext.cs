using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data;

/// <summary>EF Core DbContext with all entity sets and global query filters.</summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Map to explicit table names to ensure consistency with existing code
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<TaskItem>().ToTable("Tasks");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
        modelBuilder.Entity<Notification>().ToTable("Notifications");

        // Global soft-delete filters
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<TaskItem>().HasQueryFilter(t => !t.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>Generic repository backed by EF Core.</summary>
public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public void Update(T entity)
    {
        // If already tracked, just mark modified; otherwise attach first
        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
            DbSet.Attach(entity);
        entry.State = EntityState.Modified;
    }

    public void Delete(T entity) => DbSet.Remove(entity);
}

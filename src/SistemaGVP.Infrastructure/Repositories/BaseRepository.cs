using Microsoft.EntityFrameworkCore;
using SistemaGVP.Application.Common.Specifications;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllNoTrackingAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec)
    {
        var query = ApplySpecification(spec);
        return await query.FirstOrDefaultAsync();
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        var query = ApplySpecification(spec);
        return await query.ToListAsync();
    }

    public virtual async Task<int> CountAsync(ISpecification<T> spec)
    {
        var query = ApplySpecification(spec, countOnly: true);
        return await query.CountAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && e.IsActive);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec, bool countOnly = false)
    {
        var query = _dbSet.AsQueryable();

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        if (!countOnly && spec.AsNoTracking)
            query = query.AsNoTracking();

        if (!countOnly)
        {
            foreach (var include in spec.Includes)
                query = query.Include(include);

            if (spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDescending is not null)
                query = query.OrderByDescending(spec.OrderByDescending);
        }

        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip).Take(spec.Take);

        return query;
    }
}

using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using System.Linq.Expressions;

namespace SignMate.Infrastructure.Data;

/// <summary>
/// EF Core generic repository implementation.
/// </summary>
/// <typeparam name="T">Entity class type.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly SignMateDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(SignMateDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public IQueryable<T> Query(Expression<Func<T, bool>>? filter = null)
    {
        IQueryable<T> query = _dbSet;
        if (filter != null) query = query.Where(filter);
        return query;
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);
}

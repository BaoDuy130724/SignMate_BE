using System.Linq.Expressions;

namespace SignMate.Application.Interfaces;

/// <summary>
/// Generic repository interface for persistence operations, decouples service layer from direct DbContext.
/// </summary>
/// <typeparam name="T">Target entity type.</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

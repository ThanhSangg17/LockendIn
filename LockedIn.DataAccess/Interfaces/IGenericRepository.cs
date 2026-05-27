using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LockedIn.DataAccess.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    IQueryable<T> Query();
}

using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.Interfaces;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public virtual async Task<T?> GetByIdAsync(string id) => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.ToListAsync() : await _dbSet.Where(predicate).ToListAsync();

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.SingleOrDefaultAsync(predicate);

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Remove(T entity) => _dbSet.Remove(entity);

        public virtual void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);

        // Expose IQueryable for advanced queries
        public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

        // Paging with default ordering by Id
        //public virtual async Task<IEnumerable<T>> GetPagedAsync(
        //    int page,
        //    int pageSize,
        //    Expression<Func<T, bool>>? predicate = null,
        //    Expression<Func<T, object>>? orderBy = null)
        //{
        //    var query = predicate == null ? _dbSet.AsQueryable() : _dbSet.Where(predicate);
        //    query = orderBy != null ? query.OrderBy(orderBy) : query.OrderBy(e => EF.Property<object>(e, "Id"));
        //    return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        //}
    }
}

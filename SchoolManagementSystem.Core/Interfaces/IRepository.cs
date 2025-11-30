using System.Linq.Expressions;

namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>>? predicate = null);
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
       // Task DeleteAsync(T entity);
        //Task SoftDeleteAsync(T entity, string deletedBy);
        void RemoveRange(IEnumerable<T> entities);
        //Task<IEnumerable<T>> GetPagedAsync(
        //    int page,
        //    int pageSize,
        //    Expression<Func<T, bool>>? predicate = null,
        //    Expression<Func<T, object>>? orderBy = null
        //);

        IQueryable<T> Query(); // Expose IQueryable for advanced filtering/includes
    }
}

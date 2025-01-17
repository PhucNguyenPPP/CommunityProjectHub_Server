using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CPH.DAL.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> expression);
        Task<T?> GetByCondition(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task AddRangeAsync(List<T> entity);
        void UpdateRange(List<T> entity);
        void RemoveRange(List<T> entity);
        Task<List<T>> Paging(IQueryable<T> entity, int pageNumber, int rowsPerPage);
    }
}

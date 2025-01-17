using CPH.DAL.Context;
using CPH.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CPH.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly CphDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(CphDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public IQueryable<T> GetAllByCondition(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression).AsQueryable();
        }

        public async Task<T?> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return await _dbSet.Where(expression).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task AddRangeAsync(List<T> entity)
        {
            await _dbSet.AddRangeAsync(entity);
        }

        public void UpdateRange(List<T> entity)
        {
            _dbSet.UpdateRange(entity);
        }

        public void RemoveRange(List<T> entity)
        {
            _dbSet.RemoveRange(entity);
        }

        public async Task<List<T>> Paging(IQueryable<T> entity, int pageNumber, int rowsPerPage)
        {
            return await entity
            .Skip((pageNumber - 1) * rowsPerPage)
            .Take(rowsPerPage)
            .ToListAsync();
        }
    }
}

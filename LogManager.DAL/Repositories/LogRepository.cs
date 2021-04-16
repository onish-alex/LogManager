using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Entities;
using LogManager.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogManager.DAL.Repositories
{
    public class LogRepository<T> : IRepository<T>
        where T: BaseEntity
    {
        private LogManagerDbContext dbContext;

        public LogRepository(LogManagerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateAsync(T item)
        {
            await this.dbContext.Set<T>().AddAsync(item);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var itemToRemove = await this.dbContext.FindAsync<T>(id);
            this.dbContext.Set<T>().Remove(itemToRemove);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.dbContext.Set<T>().AnyAsync(predicate);
        }

        public async Task<T> GetByIdAsync(long id)
        {
            return await this.dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T> GetByIdAsync(long id, params Expression<Func<T, dynamic>>[] includes)
        {
            var query = this.dbContext.Set<T>().AsQueryable();

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes)
        {
            var query = this.dbContext.Set<T>().Where(predicate);

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.ToListAsync();
        }

        public async Task UpdateAsync(T item)
        {
            this.dbContext.Entry(item).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync();
        }
    }
}

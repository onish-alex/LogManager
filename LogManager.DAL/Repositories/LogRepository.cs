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
    public class LogRepository : IRepository
    {
        private LogManagerDbContext dbContext;

        public LogRepository(LogManagerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateAsync<T>(T item) where T : BaseEntity
        {
            await this.dbContext.Set<T>().AddAsync(item);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(long id) where T : BaseEntity
        {
            var itemToRemove = await this.dbContext.FindAsync<T>(id);
            this.dbContext.Set<T>().Remove(itemToRemove);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return await this.dbContext.Set<T>().AnyAsync(predicate);
        }

        public async Task<T> GetByIdAsync<T>(long id) where T : BaseEntity
        {
            return await this.dbContext.Set<T>().FindAsync(id);
        }

        public async Task<T> GetByIdAsync<T>(long id, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity
        {
            var query = this.dbContext.Set<T>().AsQueryable();

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return await this.dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity
        {
            var query = this.dbContext.Set<T>().Where(predicate);

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.ToListAsync();
        }

        public async Task<T> FindFirstAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return await this.dbContext.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task UpdateAsync<T>(T item) where T : BaseEntity
        {
            this.dbContext.Entry(item).State = EntityState.Modified;
            await this.dbContext.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await this.dbContext.SaveChangesAsync();
        }

        public void Create<T>(T item) where T : BaseEntity
        {
            this.dbContext.Set<T>().Add(item);
            this.dbContext.SaveChanges();
        }

        public T FindFirst<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return this.dbContext.Set<T>().FirstOrDefault(predicate);
        }

        public void Save()
        {
            this.dbContext.SaveChanges();
        }
    }
}

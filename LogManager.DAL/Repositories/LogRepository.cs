using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Entities;
using LogManager.Core.Utilities;
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

        public void Create<T>(T item) where T : BaseEntity
        {
            this.dbContext.Set<T>().Add(item);
        }

        public T FindFirst<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity
        {
            return this.dbContext.Set<T>().FirstOrDefault(predicate);
        }

        public void Save()
        {
            this.dbContext.SaveChanges();
        }

        public void Dispose()
        {
            this.dbContext.Dispose();
        }

        public async Task<IEnumerable<T>> GetPageAsync<T>(
            PageInfo pageInfo,
            bool isNoTracking,
            Expression<Func<T, object>> sortField,
            bool isDescending,
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity
        {
            IQueryable<T> query;

            if (isNoTracking)
            {
                query = this.dbContext.Set<T>().AsNoTracking();
            }
            else
            {
                query = this.dbContext.Set<T>();
            }

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (sortField != null)
            {
                query = (isDescending)
                    ? query.OrderByDescending(sortField)
                    : query.OrderBy(sortField);
            }

            query = query.Skip(pageInfo.PageSize * (pageInfo.PageNumber - 1)).Take(pageInfo.PageSize);

            return await query.ToListAsync();
        }

        public async Task<long> GetCountAsync<T>() where T : BaseEntity
        {
            return await this.dbContext.Set<T>().CountAsync();
        }

        public async Task<long> GetCountAsync<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity
        {
            IQueryable<T> query = this.dbContext.Set<T>().Where(predicate);

            if (includes.Length != 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.CountAsync();
        }

    }
}

using LogManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogManager.Core.Abstractions.DAL
{
    public interface IRepository : IDisposable
    {
        public Task CreateAsync<T>(T item) where T : BaseEntity;

        public Task DeleteAsync<T>(long id) where T : BaseEntity;

        public Task UpdateAsync<T>(T item) where T : BaseEntity;

        public Task<T> GetByIdAsync<T>(long id) where T : BaseEntity;

        public Task<T> GetByIdAsync<T>(long id, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;

        public Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        public Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;

        public Task<T> FindFirstAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        //public Task<IEnumerable<T>> GetPageAsync(PageInfo pageInfo, Func<T, dynamic> sortField);

        //public Task<IEnumerable<T>> GetPageAsync(PageInfo pageInfo);

        public Task<bool> ExistAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        public Task SaveAsync();


        public void Create<T>(T item) where T : BaseEntity;

        public T FindFirst<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        public void Save();
    }
}

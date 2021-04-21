using LogManager.Core.Entities;
using LogManager.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LogManager.Core.Abstractions.DAL
{
    public interface IRepository : IDisposable
    {
        Task CreateAsync<T>(T item) where T : BaseEntity;

        Task DeleteAsync<T>(long id) where T : BaseEntity;

        void Update<T>(T item) where T : BaseEntity;

        Task<T> GetByIdAsync<T>(long id) where T : BaseEntity;

        Task<T> GetByIdAsync<T>(long id, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;

        Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;

        Task<T> FindFirstAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        IEnumerable<T> GetPage<T>(
            PageInfo pageInfo,
            bool isNoTracking,
            Expression<Func<T, object>> sortField,
            bool isDescending,
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;


        Task<bool> ExistAsync<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        Task SaveAsync();

        void Create<T>(T item) where T : BaseEntity;

        T FindFirst<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        void Save();

        Task<long> GetCountAsync<T>() where T : BaseEntity;

        long GetCount<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;
    }
}

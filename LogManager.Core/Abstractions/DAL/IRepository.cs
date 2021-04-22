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
        Task<IEnumerable<T>> GetPageAsync<T>(
            PageInfo pageInfo,
            bool isNoTracking,
            Expression<Func<T, object>> sortField,
            bool isDescending,
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;

        void Create<T>(T item) where T : BaseEntity;

        T FindFirst<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity;

        void Save();

        Task<long> GetCountAsync<T>() where T : BaseEntity;

        Task<long> GetCountAsync<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, dynamic>>[] includes) where T : BaseEntity;
    }
}

using LogManager.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LogManager.Core.Abstractions.DAL
{
    public interface IRepository<T>
    {
        public Task CreateAsync(T item);

        public Task DeleteAsync(long id);

        public Task UpdateAsync(T item);

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        //public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Func<T, dynamic>[] includes);

        //public Task<IEnumerable<T>> GetPageAsync(PageInfo pageInfo, Func<T, dynamic> sortField);

        //public Task<IEnumerable<T>> GetPageAsync(PageInfo pageInfo);

        public Task<bool> ExistAsync(Expression<Func<T, bool>> predicate); 
    }
}

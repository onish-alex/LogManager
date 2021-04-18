using LogManager.Core.Abstractions.DAL;
using LogManager.DAL.Repositories;
using LogManager.DAL.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LogManager.DAL.Factories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private IDbContextFactory<LogManagerDbContext> contextFactory;

        public RepositoryFactory(IDbContextFactory<LogManagerDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        } 

        public IRepository CreateLogRepository()
        {
            return new LogRepository(this.contextFactory.CreateDbContext());
        }
    }
}

using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Settings;
using LogManager.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LogManager.DAL.Factories
{
    public class DbContextFactory : IDbContextFactory<LogManagerDbContext>
    {
        private ConnectionSettings connectionSettings;

        public DbContextFactory(IOptions<ConnectionSettings> connectionOption)
        {
            this.connectionSettings = connectionOption.Value;
        }
        
        public LogManagerDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LogManagerDbContext>()
                .UseSqlServer(this.connectionSettings.ConnectionString)
                .Options;

            return new LogManagerDbContext(options);
        }
    }
}

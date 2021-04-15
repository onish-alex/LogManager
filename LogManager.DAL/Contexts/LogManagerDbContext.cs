using LogManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogManager.DAL.Contexts
{
    public class LogManagerDbContext : DbContext
    {
        public LogManagerDbContext(DbContextOptions<LogManagerDbContext> options)
            : base(options)
        {
        }

        public DbSet<File> FileInfos { get; set; }
        
        public DbSet<Ip> IpInfos { get; set; }
        
        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ip>().Property(x => x.Address).HasMaxLength(16);
        }
    }
}

namespace LogManager.Core.Abstractions.DAL
{
    public interface IRepositoryFactory
    {
        public IRepository CreateLogRepository();
    }
}

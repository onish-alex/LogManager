using LogManager.Core.Settings;

namespace LogManager.Core.Abstractions.BLL
{
    public interface ILogParser<T>
    {
        public T Parse(string logEntry);
    }
}

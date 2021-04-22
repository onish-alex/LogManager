using LogManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogManager.Core.Abstractions.BLL
{
    public interface ILogService
    {
        double ProgressPercent { get; }

        string FileInProcess { get; }

        bool IsLoading { get; }

        void LoadLogFile(string path);

        Task<IEnumerable<File>> GetFilePage(
            int page,
            int pageSize,
            string sortField,
            bool isDescending,
            string searchText);

        Task<IEnumerable<Ip>> GetIpPage(
            int page,
            int pageSize,
            string sortField,
            bool isDescending,
            string searchText);
        
        Task<IEnumerable<LogEntry>> GetLogEntryPage(
            int page, 
            int pageSize, 
            string sortField, 
            bool isDescending, 
            string searchText);
    }
}

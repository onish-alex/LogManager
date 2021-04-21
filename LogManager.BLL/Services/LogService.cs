using LogManager.Core.Abstractions.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using LogManager.BLL.Utilities;
using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Entities;
using LogManager.Core.Utilities;
using LogManager.Core.Settings;
using FluentValidation;
using System.Threading;
using Microsoft.Extensions.Options;

using File = LogManager.Core.Entities.File;

namespace LogManager.BLL.Services
{
    public class LogService : ILogService
    {
        private ILogParser<ParsedLogEntry> parser;
        private WebHelper webHelper;
        private IValidator<ParsedLogEntry> logValidator;
        private IRepositoryFactory repositoryFactory;
        private PageSettings pageSettings;

        public double ProgressPercent => progressPercent;
        public string FileInProcess { get; private set; }
        public bool IsLoading { get; private set; }

        private double progressPercent;
        private int entriesProcessedCount;
        private int entriesCount;

        public LogService(
            ILogParser<ParsedLogEntry> parser,
            WebHelper webHelper,
            IValidator<ParsedLogEntry> logValidator,
            IRepositoryFactory repositoryFactory,
            IOptions<PageSettings> pageOptions)
        {
            this.parser = parser;
            this.webHelper = webHelper;
            this.logValidator = logValidator;
            this.repositoryFactory = repositoryFactory;

            this.pageSettings = pageOptions.Value;
        }

        public async Task LoadLogFile(string path)
        {
            var parsedData = await this.ReadLogsFromFile(path);

            progressPercent = 0;
            entriesProcessedCount = 0;
            IsLoading = true;
            entriesCount = parsedData.Count();
            FileInProcess = Path.GetFileName(path);

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            Parallel.ForEach(parsedData, options, WriteLogToDb);

            IsLoading = false;
            progressPercent = 0;
        }

        private async Task<IEnumerable<ParsedLogEntry>> ReadLogsFromFile(string path)
        {
            var parsedData = new List<ParsedLogEntry>();

            using (var reader = new StreamReader(path))
            {
                ParsedLogEntry parsedEntry;

                while (!reader.EndOfStream)
                {
                    var entry = await reader.ReadLineAsync();

                    try
                    {
                        parsedEntry = parser.Parse(entry);
                    }
                    catch
                    {
                        continue;
                    }

                    var validationResult = this.logValidator.Validate(parsedEntry);

                    if (validationResult.IsValid)
                        parsedData.Add(parsedEntry);
                }
            }

            return parsedData;
        }

        private void WriteLogToDb(ParsedLogEntry parsedLog)
        {
            using (var repository = this.repositoryFactory.CreateLogRepository())
            {
                var fileToCheck = repository.FindFirst<File>(x => x.Path == parsedLog.Path);

                if (fileToCheck == null)
                {
                    WebPageInfo pageInfo;

                    try
                    {
                        pageInfo = webHelper.GetPageInfo(parsedLog.Path);

                        fileToCheck = new File()
                        {
                            Path = parsedLog.Path,
                            Size = pageInfo.Size,
                            Title = pageInfo.Title,
                        };
                    }
                    catch
                    {
                        fileToCheck = new File()
                        {
                            Path = parsedLog.Path,
                        };
                    }

                    repository.Create(fileToCheck);
                }

                var ipInBytes = IpConverter.FromString(parsedLog.IpAddress);
                var ipToCheck = repository.FindFirst<Ip>(x => x.Address.SequenceEqual(ipInBytes));

                if (ipToCheck == null)
                {
                    var organization = webHelper.GetOrganizationNameByWhois(parsedLog.IpAddress);
                    ipToCheck = new Ip()
                    {
                        Address = ipInBytes,
                        OwnerName = organization,
                    };
                   
                    repository.Create(ipToCheck);
                }

                var logEntry = new LogEntry()
                {
                    IpInfo = ipToCheck,
                    FileInfo = fileToCheck,
                    Method = parsedLog.Method,
                    Amount = int.Parse(parsedLog.Amount),
                    StatusCode = short.Parse(parsedLog.Status),
                    Date = DateTimeOffset.ParseExact(
                        parsedLog.Date,
                        LogSettings.DateTimeOffsetPattern,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None),
                };

                repository.Create(logEntry);

                try
                {
                    repository.Save();
                }
                catch
                {
                    WriteLogToDb(parsedLog);
                    return;
                }

                Interlocked.Increment(ref entriesProcessedCount);
                Interlocked.Exchange(ref progressPercent,((double)entriesProcessedCount / entriesCount * 100));
            }
        }

        public async Task<IEnumerable<File>> GetFilePage(
            int page, 
            int pageSize, 
            string sortField,
            bool isDescending,
            string searchText)
        {
            using (var repository = repositoryFactory.CreateLogRepository())
            {
                var pageInfo = new PageInfo()
                {
                    PageNumber = page,
                    PageSize = pageSize,
                };

                IEnumerable<File> filePage;

                var pageTotalCount = string.IsNullOrEmpty(searchText)
                    ? (int)Math.Ceiling(await repository.GetCountAsync<File>() / (double)pageSize)
                    : (int)Math.Ceiling(repository.GetCount<File>(
                    ExpressionHelper.GetSearchForFile(searchText)) 
                    / (double)pageSize);

                if (pageTotalCount == 0)
                {
                    pageTotalCount++;
                }
               
                if (string.IsNullOrEmpty(searchText))
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForFile(sortField),
                        isDescending,
                        null);
                }
                else
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForFile(sortField),
                        isDescending,
                        ExpressionHelper.GetSearchForFile(searchText));
                }

                var paginatedList = GeneratePaginatedList(filePage, page, pageSize, pageTotalCount);

                paginatedList.IsDescending = isDescending;
                paginatedList.SortField = sortField;
                paginatedList.SearchText = searchText;

                return paginatedList;
            }
        }

        public async Task<IEnumerable<Ip>> GetIpPage(
            int page,
            int pageSize,
            string sortField,
            bool isDescending,
            string searchText)
        {
            using (var repository = repositoryFactory.CreateLogRepository())
            {
                var pageInfo = new PageInfo()
                {
                    PageNumber = page,
                    PageSize = pageSize,
                };

                IEnumerable<Ip> ipPage;

                var pageTotalCount = string.IsNullOrEmpty(searchText)
                    ? (int)Math.Ceiling(await repository.GetCountAsync<Ip>() / (double)pageSize)
                    : (int)Math.Ceiling(repository.GetCount<Ip>(
                          ExpressionHelper.GetSearchForIp(searchText)) 
                    / (double)pageSize);

                if (pageTotalCount == 0)
                {
                    pageTotalCount++;
                }

                if (string.IsNullOrEmpty(searchText))
                {
                    ipPage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForIp(sortField),
                        isDescending,
                        null);
                }
                else
                {
                    ipPage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForIp(sortField),
                        isDescending,
                        ExpressionHelper.GetSearchForIp(searchText));
                }

                var paginatedList = GeneratePaginatedList(ipPage, page, pageSize, pageTotalCount);

                paginatedList.IsDescending = isDescending;
                paginatedList.SortField = sortField;
                paginatedList.SearchText = searchText;

                return paginatedList;
            }
        }

        public async Task<IEnumerable<LogEntry>> GetLogEntryPage(
            int page,
            int pageSize,
            string sortField,
            bool isDescending,
            string searchText)
        {
            using (var repository = repositoryFactory.CreateLogRepository())
            {
                var pageInfo = new PageInfo()
                {
                    PageNumber = page,
                    PageSize = pageSize,
                };

                IEnumerable<LogEntry> logPage;

                if (string.IsNullOrEmpty(searchText))
                {
                    logPage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForLog(sortField),
                        isDescending,
                        null,
                        x => x.IpInfo,
                        x => x.FileInfo);
                }
                else
                {
                    logPage = repository.GetPage(
                        pageInfo,
                        true,
                        ExpressionHelper.GetSortForLog(sortField),
                        isDescending,
                        ExpressionHelper.GetSearchForLog(searchText),
                        x => x.IpInfo,
                        x => x.FileInfo);
                }

                var pageTotalCount = string.IsNullOrEmpty(searchText)
                    ? (int)Math.Ceiling(await repository.GetCountAsync<LogEntry>() / (double)pageSize)
                    : (int)Math.Ceiling(repository.GetCount(
                        ExpressionHelper.GetSearchForLog(searchText),
                        x => x.IpInfo,
                        x => x.FileInfo)
                    / (double)pageSize);

                if (pageTotalCount == 0)
                {
                    pageTotalCount++;
                }

                var paginatedList = GeneratePaginatedList(logPage, page, pageSize, pageTotalCount);

                paginatedList.IsDescending = isDescending;
                paginatedList.SortField = sortField;
                paginatedList.SearchText = searchText;

                return paginatedList;
            }
        }
        
        private PaginatedList<T> GeneratePaginatedList<T>(
            IEnumerable<T> pageData,
            int page, 
            int pageSize, 
            int pageTotalCount)
        {
            var minPage = (page - 1 < pageSettings.LinkCount / 2)
                    ? page - 1
                    : pageSettings.LinkCount / 2;

            var maxPage = (pageTotalCount - page < pageSettings.LinkCount - minPage)
                ? pageTotalCount - page
                : pageSettings.LinkCount - minPage;

            minPage = (page - 1 < pageSettings.LinkCount - maxPage)
                ? page - 1
                : pageSettings.LinkCount - maxPage;

            return new PaginatedList<T>(
                pageData,
                page,
                pageSize,
                pageTotalCount,
                minPage, maxPage);
        }   
    }
}

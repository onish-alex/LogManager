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

using File = LogManager.Core.Entities.File;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Linq.Expressions;

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
                        x => x.Path.Contains(searchText)
                          || (x.Title != null && x.Title.Contains(searchText))) 
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
                        SortExpressions.ForFile(sortField),
                        isDescending,
                        null);
                }
                else
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        SortExpressions.ForFile(sortField),
                        isDescending,
                        x => x.Path.Contains(searchText) 
                         || (x.Title != null && x.Title.Contains(searchText)));
                }

                var minPage = (page - 1 < pageSettings.LinkCount / 2)
                    ? page - 1
                    : pageSettings.LinkCount / 2;

                var maxPage = (pageTotalCount - page < pageSettings.LinkCount - minPage)
                    ? pageTotalCount - page
                    : pageSettings.LinkCount - minPage;

                minPage = (page - 1 < pageSettings.LinkCount - maxPage)
                    ? page - 1
                    : pageSettings.LinkCount - maxPage;

                var paginatedList = new PaginatedList<File>(
                    filePage,
                    page,
                    pageSize,
                    pageTotalCount, 
                    minPage, maxPage);

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

                IEnumerable<Ip> filePage;

                var pageTotalCount = string.IsNullOrEmpty(searchText)
                    ? (int)Math.Ceiling(await repository.GetCountAsync<Ip>() / (double)pageSize)
                    : (int)Math.Ceiling(repository.GetCount<Ip>(
                          x => x.OwnerName != null && x.OwnerName.Contains(searchText)) 
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
                        SortExpressions.ForIp(sortField),
                        isDescending,
                        null);
                }
                else
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        SortExpressions.ForIp(sortField),
                        isDescending,
                        x => x.OwnerName != null && x.OwnerName.Contains(searchText));
                }

                var minPage = (page - 1 < pageSettings.LinkCount / 2)
                    ? page - 1
                    : pageSettings.LinkCount / 2;

                var maxPage = (pageTotalCount - page < pageSettings.LinkCount - minPage)
                    ? pageTotalCount - page
                    : pageSettings.LinkCount - minPage;

                minPage = (page - 1 < pageSettings.LinkCount - maxPage)
                    ? page - 1
                    : pageSettings.LinkCount - maxPage;

                var paginatedList = new PaginatedList<Ip>(
                    filePage,
                    page,
                    pageSize,
                    pageTotalCount,
                    minPage, maxPage);

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

                IEnumerable<LogEntry> filePage;

                if (string.IsNullOrEmpty(searchText))
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        SortExpressions.ForLog(sortField),
                        isDescending,
                        null,
                        x => x.IpInfo,
                        x => x.FileInfo);
                }
                else
                {
                    filePage = repository.GetPage(
                        pageInfo,
                        true,
                        SortExpressions.ForLog(sortField),
                        isDescending,
                        x => x.Date.ToString().Contains(searchText)
                          || x.StatusCode.ToString().Contains(searchText)
                          || (x.Method != null && x.Method.Contains(searchText))
                          || x.FileInfo.Path.Contains(searchText)
                          || x.Amount.ToString().Contains(searchText),
                        x => x.IpInfo,
                        x => x.FileInfo);
                }

                var pageTotalCount = string.IsNullOrEmpty(searchText)
                    ? (int)Math.Ceiling(await repository.GetCountAsync<LogEntry>() / (double)pageSize)
                    : (int)Math.Ceiling(repository.GetCount<LogEntry>(
                        x => x.Date.ToString().Contains(searchText)
                          || x.StatusCode.ToString().Contains(searchText)
                          || (x.Method != null && x.Method.Contains(searchText))
                          || x.FileInfo.Path.Contains(searchText)
                          || x.Amount.ToString().Contains(searchText),
                        x => x.IpInfo,
                        x => x.FileInfo)
                    / (double)pageSize);

                if (pageTotalCount == 0)
                {
                    pageTotalCount++;
                }

                var minPage = (page - 1 < pageSettings.LinkCount / 2)
                    ? page - 1
                    : pageSettings.LinkCount / 2;

                var maxPage = (pageTotalCount - page < pageSettings.LinkCount - minPage)
                    ? pageTotalCount - page
                    : pageSettings.LinkCount - minPage;

                minPage = (page - 1 < pageSettings.LinkCount - maxPage)
                    ? page - 1
                    : pageSettings.LinkCount - maxPage;

                var paginatedList = new PaginatedList<LogEntry>(
                    filePage,
                    page,
                    pageSize,
                    pageTotalCount,
                    minPage, maxPage);

                paginatedList.IsDescending = isDescending;
                paginatedList.SortField = sortField;
                paginatedList.SearchText = searchText;

                return paginatedList;
            }

            
        }

        private object FindPropertyValue(object source, string[] propertyChain)
        {
            //var properties = sortField.Split('.', StringSplitOptions.RemoveEmptyEntries);
            PropertyInfo property = source.GetType().GetProperty(propertyChain[0]);
            object entity = source;

            for (var i = 0; i < propertyChain.Length; i++)
            {
                property = entity.GetType().GetProperty(propertyChain[i]);
                entity = property.GetValue(entity);
            }

            return entity;
        }
    }
}

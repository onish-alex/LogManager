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

namespace LogManager.BLL.Services
{
    public class LogService : ILogService
    {
        private ILogParser<ParsedLogEntry> parser;
        private WebHelper webHelper;
        private IValidator<ParsedLogEntry> logValidator;
        private IRepositoryFactory repositoryFactory;
        //private IRepository<Ip> ipRepository;
        //private IRepository<File> fileRepository;
        //private IRepository<LogEntry> logEntryRepository;

        public LogService(
            ILogParser<ParsedLogEntry> parser,
            WebHelper webHelper,
            IValidator<ParsedLogEntry> logValidator,
            IRepositoryFactory repositoryFactory
            //IRepository<Ip> ipRepository,
            //IRepository<File> fileRepository,
            //IRepository<LogEntry> logEntryRepository
            )
        {
            this.parser = parser;
            this.webHelper = webHelper;
            this.logValidator = logValidator;
            this.repositoryFactory = repositoryFactory;
            //this.ipRepository = ipRepository;
            //this.fileRepository = fileRepository;
            //this.logEntryRepository = logEntryRepository;
        }

        public void LoadLogFile(string path)
        {
            var parsedData = this.ReadLogsFromFile(path);
            Parallel.ForEach(parsedData, WriteLogToDb);
        }

        private IEnumerable<ParsedLogEntry> ReadLogsFromFile(string path)
        {
            var parsedData = new List<ParsedLogEntry>();

            var reader = new StreamReader(path);
            
                ParsedLogEntry parsedEntry;

                while (!reader.EndOfStream)
                {
                    var entry = reader.ReadLine();
                    
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

            reader.Dispose();

            return parsedData;
        }

        private void WriteLogToDb(ParsedLogEntry parsedLog)
        {
            var repository = this.repositoryFactory.CreateLogRepository();

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

                try
                {
                    repository.Create(fileToCheck);
                }
                catch
                {
                    WriteLogToDb(parsedLog);
                }
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

                try
                {
                    repository.Create(ipToCheck);
                }
                catch
                {
                    WriteLogToDb(parsedLog);
                }
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

            try
            {
                repository.Create(logEntry);
            }
            catch
            {
                WriteLogToDb(parsedLog);
            }

            //repository.Save();
        }
    }
}

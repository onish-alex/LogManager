using LogManager.Core.Abstractions.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LogManager.BLL.Utilities;
using LogManager.Core.Abstractions.DAL;
using LogManager.Core.Entities;
using LogManager.Core.Utilities;
using Microsoft.Extensions.Options;

using File = LogManager.Core.Entities.File;
using LogManager.Core.Settings;
using FluentValidation;

namespace LogManager.BLL.Services
{
    public class LogService : ILogService
    {
        private ILogParser<ParsedLogEntry> parser;
        private WebHelper webHelper;
        private IValidator<ParsedLogEntry> logValidator;
        private IRepository<Ip> ipRepository;
        private IRepository<File> fileRepository;
        private IRepository<LogEntry> logEntryRepository;

        public LogService(
            ILogParser<ParsedLogEntry> parser,
            WebHelper webHelper,
            IValidator<ParsedLogEntry> logValidator,
            IRepository<Ip> ipRepository,
            IRepository<File> fileRepository,
            IRepository<LogEntry> logEntryRepository)
        {
            this.parser = parser;
            this.webHelper = webHelper;
            this.logValidator = logValidator;
            this.ipRepository = ipRepository;
            this.fileRepository = fileRepository;
            this.logEntryRepository = logEntryRepository;
        }

        public async Task LoadLogFile(string path)
        {
            var parsedData = await this.ReadLogsFromFile(path);

            foreach(var item in parsedData)
            {
                var ipInBytes = IpConverter.FromString(item.IpAddress);
                var ipToCheck = (await ipRepository.FindAsync(x => x.Address.SequenceEqual(ipInBytes))).FirstOrDefault();

                if (ipToCheck == null)
                {
                    var organization = await webHelper.GetOrganizationNameByWhoisAsync(item.IpAddress);
                    ipToCheck = new Ip()
                    {
                        Address = ipInBytes,
                        OwnerName = organization,
                    };

                    await ipRepository.CreateAsync(ipToCheck);
                }

                var fileToCheck = (await fileRepository.FindAsync(x => x.Path == item.Path)).FirstOrDefault();
                
                if (fileToCheck == null)
                {
                    var pageInfo = webHelper.GetPageInfo(item.Path);
                    
                    fileToCheck = new File()
                    {
                        Path = item.Path,
                        Size = pageInfo.Size,
                        Title = pageInfo.Title,
                    };

                    await fileRepository.CreateAsync(fileToCheck);
                }

                var logEntry = new LogEntry()
                {
                    IpInfo = ipToCheck,
                    FileInfo = fileToCheck,
                    Method = item.Method,
                    Amount = int.Parse(item.Amount),
                    StatusCode = short.Parse(item.Status),
                    Date = DateTimeOffset.ParseExact(
                        item.Date,
                        LogSettings.DateTimeOffsetPattern,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None),
                };

                await this.logEntryRepository.CreateAsync(logEntry);
            }
        }

        private async Task<IEnumerable<ParsedLogEntry>> ReadLogsFromFile(string path)
        {
            var parsedData = new List<ParsedLogEntry>();

            var reader = new StreamReader(path);
            
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

            reader.Dispose();

            return parsedData;
        }
    }
}

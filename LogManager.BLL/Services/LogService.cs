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
using Whois.NET;

using File = LogManager.Core.Entities.File;

namespace LogManager.BLL.Services
{
    public class LogService : ILogService
    {
        private ILogParser<ParsedLogEntry> parser;
        private IRepository<Ip> IpRepository;
        private IRepository<File> FileRepository;
        private IRepository<LogEntry> LogEntryRepository;

        public LogService(
            ILogParser<ParsedLogEntry> parser,
            
            IRepository<Ip> ipRepository,
            IRepository<File> fileRepository,
            IRepository<LogEntry> logEntryRepository)
        {
            this.parser = parser;
            this.IpRepository = ipRepository;
            this.FileRepository = fileRepository;
            this.LogEntryRepository = logEntryRepository;
        }

        public async Task LoadLogFile(string path)
        {
            var parsedData = this.ReadFromFile(path);

            foreach(var item in parsedData)
            {
                var ipInBytes = IpConverter.FromString(item.IpAddress);
                var ipToCheck = (await IpRepository.FindAsync(x => x.Address.SequenceEqual(ipInBytes))).FirstOrDefault();

                if (ipToCheck == null)
                {
                    var whoisResponse = await WhoisClient.QueryAsync(item.IpAddress);
                    ipToCheck = new Ip()
                    {
                        Address = ipInBytes,
                        OwnerName = whoisResponse.OrganizationName,

                    };

                    await IpRepository.CreateAsync(ipToCheck);
                }

                var fileToCheck = (await FileRepository.FindAsync(x => x.Path == item.Path)).FirstOrDefault();
                
                if (fileToCheck == null)
                {


                    fileToCheck = new File()
                    {
                        Path = item.Path,
                        //TODO insert title
                    };
                }

            }
        }

        public IEnumerable<ParsedLogEntry> ReadFromFile(string path)
        {
            var parsedData = new List<ParsedLogEntry>();

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var entry = reader.ReadLine();
                    var parsedEntry = parser.Parse(entry);
                    parsedData.Add(parsedEntry);
                }
            }

            return parsedData;
        }
    }
}

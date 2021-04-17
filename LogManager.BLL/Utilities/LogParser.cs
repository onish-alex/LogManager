using LogManager.Core.Abstractions.BLL;
using LogManager.BLL.Enumerations;
using System;
using LogManager.Core.Settings;
using Microsoft.Extensions.Options;
using System.Linq;
using System.IO;
using LogManager.Core.Resources;

namespace LogManager.BLL.Utilities
{
    public class LogParser : ILogParser<ParsedLogEntry>
    {
        private RequestSettings requestSettings;

        public LogParser(IOptionsSnapshot<RequestSettings> requestSettingOptions)
        {
            this.requestSettings = requestSettingOptions.Value;
        }
        
        public ParsedLogEntry Parse(string logEntry)
        {
            var entryElements = logEntry.Split(new char[] { '\"', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (entryElements.Length != LogSettings.EntryPartsCount)
                throw new ArgumentException(ErrorMessages.InvalidLogFormat);

            var parsedLog = new ParsedLogEntry()
            {
                IpAddress = entryElements[(int)LogEntryElementPosition.Ip],
                Date      = entryElements[(int)LogEntryElementPosition.Date] + " " + entryElements[(int)LogEntryElementPosition.Timezone],
                Method    = entryElements[(int)LogEntryElementPosition.Method],
                Amount    = entryElements[(int)LogEntryElementPosition.Amount],
                Path      = entryElements[(int)LogEntryElementPosition.Path],
                Status    = entryElements[(int)LogEntryElementPosition.Status], 
            };

            return parsedLog;
        }
    }
}

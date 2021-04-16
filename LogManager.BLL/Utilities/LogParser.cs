using LogManager.Core.Abstractions.BLL;
using LogManager.BLL.Enumerations;
using System;

namespace LogManager.BLL.Utilities
{
    public class LogParser : ILogParser<ParsedLogEntry>
    {
        public ParsedLogEntry Parse(string logEntry)
        {
            var entryElements = logEntry.Split(new char[] { '\"', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var parsedLog = new ParsedLogEntry()
            {
                IpAddress = entryElements[(int)LogEntryElementPosition.IP],
                Date      = entryElements[(int)LogEntryElementPosition.Date],
                Method    = entryElements[(int)LogEntryElementPosition.Method],
                Amount    = entryElements[(int)LogEntryElementPosition.Amount],
                Path      = entryElements[(int)LogEntryElementPosition.Path],
                Status    = entryElements[(int)LogEntryElementPosition.Status],
            };

            return parsedLog;
        }
    }
}

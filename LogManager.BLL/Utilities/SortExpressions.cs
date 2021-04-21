using LogManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LogManager.BLL.Utilities
{
    public static class SortExpressions
    {
        private static Dictionary<string, Expression<Func<Ip, dynamic>>> ipExpressions = new Dictionary<string, Expression<Func<Ip, dynamic>>>()
        {
            { "OwnerName", x => x.OwnerName },
        };

        private static Dictionary<string, Expression<Func<File, dynamic>>> fileExpressions = new Dictionary<string, Expression<Func<File, dynamic>>>()
        {
            { "Path", x => x.Path },
            { "Size", x => x.Size },
            { "Title", x => x.Title },
        };

        private static Dictionary<string, Expression<Func<LogEntry, dynamic>>> logEntryExpressions = new Dictionary<string, Expression<Func<LogEntry, dynamic>>>()
        {
            { "Amount", x => x.Amount },
            { "Date", x => x.Date },
            { "FileInfo.Path", x => x.FileInfo.Path },
            { "FileInfo.Title", x => x.FileInfo.Title },
            { "IpInfo.OwnerName", x => x.IpInfo.OwnerName },
            { "Method", x => x.Method },
            { "StatusCode", x => x.StatusCode },
        };

        public static Expression<Func<LogEntry, dynamic>> ForLog(string sortName)
        {
            return logEntryExpressions[sortName];
        }

        public static Expression<Func<File, dynamic>> ForFile(string sortName)
        {
            return fileExpressions[sortName];
        }

        public static Expression<Func<Ip, dynamic>> ForIp(string sortName)
        {
            return ipExpressions[sortName];
        }
    }
}

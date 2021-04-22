using LogManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogManager.BLL.Utilities
{
    public static class ExpressionHelper
    {
        private static Dictionary<string, Expression<Func<Ip, dynamic>>> ipExpressions = new Dictionary<string, Expression<Func<Ip, dynamic>>>()
        {
            { "OwnerName", x => x.OwnerName },
            { "Address", x => x.Address },
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
            { "IpInfo.Address", x => x.IpInfo.Address },
            { "Method", x => x.Method },
            { "StatusCode", x => x.StatusCode },
        };

        public static Expression<Func<LogEntry, dynamic>> GetSortForLog(string sortName)
        {
            return logEntryExpressions[sortName];
        }

        public static Expression<Func<File, dynamic>> GetSortForFile(string sortName)
        {
            return fileExpressions[sortName];
        }

        public static Expression<Func<Ip, dynamic>> GetSortForIp(string sortName)
        {
            return ipExpressions[sortName];
        }

        public static Expression<Func<LogEntry, bool>> GetSearchForLog(string searchText)
        {
            return x => x.Date.ToString().Contains(searchText)
                          || x.StatusCode.ToString().Contains(searchText)
                          || (x.Method != null && x.Method.Contains(searchText))
                          || x.FileInfo.Path.Contains(searchText)
                          || (x.FileInfo.Title != null && x.FileInfo.Title.Contains(searchText))
                          || (x.IpInfo.OwnerName != null && x.IpInfo.OwnerName.Contains(searchText))
                          || x.Amount.ToString().Contains(searchText);
        }

        public static Expression<Func<Ip, bool>> GetSearchForIp(string searchText)
        {
            return x => x.OwnerName != null && x.OwnerName.Contains(searchText);
        }

        public static Expression<Func<File, bool>> GetSearchForFile(string searchText)
        {
            return x => x.Path.Contains(searchText)
                    || (x.Title != null && x.Title.Contains(searchText));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogManager.Core.Abstractions.BLL
{
    public interface ILogService
    {
        public Task LoadLogFile(string path);
    }
}

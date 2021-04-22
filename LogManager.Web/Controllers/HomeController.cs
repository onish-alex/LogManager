using LogManager.BLL.Utilities;
using LogManager.Core.Abstractions.BLL;
using LogManager.Core.Entities;
using LogManager.Core.Resources;
using LogManager.Core.Settings;
using LogManager.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using File = LogManager.Core.Entities.File;

namespace LogManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private string pathToSave;
        private ILogService logService;

        public HomeController(
            IOptionsSnapshot<FileSettings> fileOptions,
            ILogService logService)
        {
            this.pathToSave = fileOptions.Value.PathToSave;
            this.logService = logService;
        }

        public async Task<IActionResult> LogEntry(
            int page = 1,
            int pageSize = 30,
            string sortField = "Date",
            bool isDescending = false,
            string searchText = null)
        {
            var logEntryPage = await logService.GetLogEntryPage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<LogEntry>;

            return View(logEntryPage);
        }

        public async Task<IActionResult> LogEntryAjax(
            int page = 1,
            int pageSize = 30,
            string sortField = "Date",
            bool isDescending = false,
            string searchText = null)
        {
            var logEntryPage = await logService.GetLogEntryPage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<LogEntry>;

            return PartialView("_LogEntryPartial", logEntryPage);
        }

        public async Task<IActionResult> Ip(
            int page = 1,
            int pageSize = 30,
            string sortField = "Address",
            bool isDescending = false,
            string searchText = null)
        {
            var ipPage = await logService.GetIpPage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<Ip>;

            return View(ipPage);
        }

        public async Task<IActionResult> IpAjax(
            int page = 1,
            int pageSize = 30,
            string sortField = "Address",
            bool isDescending = false,
            string searchText = null)
        {
            var ipPage = await logService.GetIpPage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<Ip>;

            return PartialView("_IpPartial", ipPage);
        }

        public async Task<IActionResult> File(
            int page = 1, 
            int pageSize = 30, 
            string sortField = "Path",
            bool isDescending = false,
            string searchText = null)
        {
            var filePage = await logService.GetFilePage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<File>;
            
            return View(filePage);
        }

        public async Task<IActionResult> FileAjax(
            int page = 1,
            int pageSize = 30,
            string sortField = "Path",
            bool isDescending = false,
            string searchText = null)
        {
            var filePage = await logService.GetFilePage(page, pageSize, sortField, isDescending, searchText) as PaginatedList<File>;

            return PartialView("_FilePartial", filePage);
        }

        [HttpGet]
        public IActionResult Load()
        {
            var viewModel = new LoadViewModel()
            {
                IsLoad = logService.IsLoading,
                FileName = logService.IsLoading ? logService.FileInProcess : string.Empty,
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Load(IFormFile file)
        {
            if (file == null)
            {
                var viewModel = new LoadViewModel()
                {
                    IsLoad = logService.IsLoading,
                    FileName = logService.IsLoading ? logService.FileInProcess : string.Empty,
                };

                return View(viewModel);
            }

            if (!logService.IsLoading)
            {
                using (var stream = new FileStream(this.pathToSave + file.FileName, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                try
                {
                    logService.LoadLogFile(this.pathToSave + file.FileName);
                }
                catch
                {
                    var exceptionViewModel = new LoadViewModel()
                    {
                        IsLoad = false,
                        FileName = file.FileName,
                        Message = ErrorMessages.FileWithoutLogs
                    };

                    return View(exceptionViewModel);
                }

                var viewModel = new LoadViewModel()
                {
                    IsLoad = true,
                    FileName = file.FileName,
                };

                return View(viewModel);
            } 
            else
            {
                var viewModel = new LoadViewModel()
                {
                    IsLoad = logService.IsLoading,
                    FileName = logService.IsLoading ? logService.FileInProcess : string.Empty,
                };

                return View(viewModel);
            }
        }

        public ActionResult<double> GetLoadProgress()
        {
            return new ActionResult<double>(logService.ProgressPercent);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

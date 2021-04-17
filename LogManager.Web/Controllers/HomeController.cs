using LogManager.Core.Abstractions.BLL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private ILogService logService;

        public HomeController(ILogService logService)
        {
            this.logService = logService;
        }

        public IActionResult Index()
        {
            this.logService.LoadLogFile(@"D:\Work\SoftPI\testLog.txt");


            return View();
        }
    }
}

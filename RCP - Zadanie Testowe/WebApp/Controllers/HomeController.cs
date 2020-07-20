using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommonCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Log(int? id = null)
        {
            var recordsList = await DatabaseOperator.DownloadRecordsAsList(id);

            var firstRecord = recordsList.FirstOrDefault();
            int firstId = firstRecord is null ? -1 : (int)firstRecord.RecordId;

            int inc = DatabaseOperator.defaultRecordsIncrement;

            if (firstId > -1)
            {
                ViewData["idFirst"] = firstId;
                ViewData["idPrev"] = firstId - inc;
                ViewData["idNext"] = id is null ? (int?)null : firstId + inc;
            }
            else
            {
                ViewData["idFirst"] = null;
                ViewData["idPrev"] = 1;
                ViewData["idNext"] = null;
            }

            return View(recordsList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

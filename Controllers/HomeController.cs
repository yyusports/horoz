using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sportify.Models;
using Sportify.Services;

namespace Sportify.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISportDataService _sport;

        public HomeController(ISportDataService sport)
        {
            _sport = sport;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _sport.GetDashboardAsync();
            return View(dashboard);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

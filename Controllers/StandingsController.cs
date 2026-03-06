using Microsoft.AspNetCore.Mvc;
using Sportify.Services;

namespace Sportify.Controllers
{
    public class StandingsController : Controller
    {
        private readonly ISportDataService _sport;

        public StandingsController(ISportDataService sport)
        {
            _sport = sport;
        }

        public async Task<IActionResult> Index(int leagueId = 39, int season = 2024)
        {
            var standings = await _sport.GetStandingsAsync(leagueId, season);
            ViewBag.LeagueId = leagueId;
            ViewBag.Season = season;
            return View(standings);
        }
    }
}

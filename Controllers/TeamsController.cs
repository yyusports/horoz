using Microsoft.AspNetCore.Mvc;
using Sportify.Services;

namespace Sportify.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ISportDataService _sport;

        public TeamsController(ISportDataService sport)
        {
            _sport = sport;
        }

        public async Task<IActionResult> Index(int leagueId = 39, int season = 2024)
        {
            var teams = await _sport.GetTeamsAsync(leagueId, season);
            ViewBag.LeagueId = leagueId;
            ViewBag.Season = season;
            return View(teams);
        }

        public async Task<IActionResult> Detail(int id, int leagueId = 39, int season = 2024)
        {
            var team = await _sport.GetTeamDetailsAsync(id, leagueId, season);
            if (team == null) return NotFound();
            return View(team);
        }
    }
}

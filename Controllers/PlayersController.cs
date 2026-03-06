using Microsoft.AspNetCore.Mvc;
using Sportify.Services;

namespace Sportify.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ISportDataService _sport;

        public PlayersController(ISportDataService sport)
        {
            _sport = sport;
        }

        public async Task<IActionResult> Index(int leagueId = 39, int season = 2024)
        {
            var players = await _sport.GetTopScorersAsync(leagueId, season);
            ViewBag.LeagueId = leagueId;
            ViewBag.Season = season;
            return View(players);
        }

        public async Task<IActionResult> Detail(int id, int season = 2024)
        {
            var player = await _sport.GetPlayerDetailsAsync(id, season);
            if (player == null) return NotFound();
            return View(player);
        }
    }
}

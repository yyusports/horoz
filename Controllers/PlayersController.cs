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
            var oyuncular = await _sport.GetTopScorersAsync(leagueId, season);
            ViewBag.LeagueId = leagueId;
            ViewBag.Season = season;
            return View(oyuncular);
        }

        public async Task<IActionResult> Detail(int id, int season = 2024)
        {
            var oyuncu = await _sport.GetPlayerDetailsAsync(id, season);
            if (oyuncu == null) return NotFound();
            return View(oyuncu);
        }
    }
}

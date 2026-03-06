using Microsoft.AspNetCore.Mvc;
using Sportify.Services;

namespace Sportify.Controllers
{
    [Route("api/sport")]
    [ApiController]
    public class SportApiController : ControllerBase
    {
        private readonly ISportDataService _sport;

        public SportApiController(ISportDataService sport)
        {
            _sport = sport;
        }

        [HttpGet("live")]
        public async Task<IActionResult> LiveMatches()
        {
            var matches = await _sport.GetLiveMatchesAsync();
            return Ok(new { data = matches, updatedAt = DateTime.Now });
        }

        [HttpGet("today")]
        public async Task<IActionResult> TodayMatches()
        {
            var matches = await _sport.GetTodayMatchesAsync();
            return Ok(new { data = matches, updatedAt = DateTime.Now });
        }

        [HttpGet("standings")]
        public async Task<IActionResult> Standings([FromQuery] int leagueId = 39, [FromQuery] int season = 2024)
        {
            var standings = await _sport.GetStandingsAsync(leagueId, season);
            return Ok(new { data = standings, updatedAt = DateTime.Now });
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var dashboard = await _sport.GetDashboardAsync();
            return Ok(new { data = dashboard, updatedAt = DateTime.Now });
        }
    }
}

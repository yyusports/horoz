using Microsoft.AspNetCore.Mvc;
using Sportify.Services;
using Sportify.Repositories;
using Sportify.Models;

namespace Sportify.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ISportDataService _sport;
        private readonly IUserRepository _userRepository;

        public TeamsController(ISportDataService sport, IUserRepository userRepository)
        {
            _sport = sport;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var majorLeagues = new Dictionary<int, string>
            {
                { 39, "🏴󠁧󠁢󠁥󠁮󠁧󠁿 Premier League" },
                { 140, "🇪🇸 La Liga" },
                { 78, "🇩🇪 Bundesliga" },
                { 135, "🇮🇹 Serie A" },
                { 61, "🇫🇷 Ligue 1" },
                { 203, "🇹🇷 Süper Lig" }
            };

            var viewModel = new TeamsViewModel();
            var tasks = majorLeagues.Keys.Select(id => _sport.GetTeamsAsync(id, 2024));
            var results = await Task.WhenAll(tasks);

            int i = 0;
            foreach (var leagueId in majorLeagues.Keys)
            {
                viewModel.LeaguedTeams.Add(majorLeagues[leagueId], results[i] ?? new List<Takim>());
                i++;
            }
            
            if (User.Identity?.IsAuthenticated == true)
            {
                var kullanici = await _userRepository.KullaniciAdiIleGetirAsync(User.Identity.Name!);
                if (kullanici != null)
                {
                    var favoriler = await _userRepository.GetFavoriteTeamsAsync(kullanici.Id);
                    viewModel.FavoriteTeamIds = favoriler.Select(f => f.Id).ToHashSet();
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Detail(int id, int leagueId = 39, int season = 2024)
        {
            var takim = await _sport.GetTeamDetailsAsync(id, leagueId, season);
            if (takim == null) return NotFound();
            return View(takim);
        }
    }
}

using System.Text.Json;
using Sportify.Models;

namespace Sportify.Services
{
    public interface ISportDataService
    {
        Task<List<Standing>> GetStandingsAsync(int leagueId, int season);
        Task<List<Player>> GetTopScorersAsync(int leagueId, int season);
        Task<List<Match>> GetLiveMatchesAsync();
        Task<List<Match>> GetTodayMatchesAsync();
        Task<List<Team>> GetTeamsAsync(int leagueId, int season);
        Task<Team?> GetTeamDetailsAsync(int teamId, int leagueId, int season);
        Task<Player?> GetPlayerDetailsAsync(int playerId, int season);
        Task<DashboardViewModel> GetDashboardAsync();
    }

    public class SportDataService : ISportDataService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<SportDataService> _logger;
        private readonly string _apiKey;
        private const string BaseUrl = "https://v3.football.api-sports.io";

        public SportDataService(HttpClient http, IConfiguration config, ILogger<SportDataService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
            _apiKey = _config["ApiFootball:ApiKey"] ?? "";

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("x-apisports-key", _apiKey);
            }
        }

        private bool HasApiKey => !string.IsNullOrEmpty(_apiKey) && _apiKey != "YOUR_API_KEY_HERE";

        private async Task<JsonDocument?> GetAsync(string endpoint)
        {
            try
            {
                var response = await _http.GetAsync($"{BaseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("API call failed: {Error}", ex.Message);
                return null;
            }
        }

        public async Task<List<Standing>> GetStandingsAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockStandings();

            var doc = await GetAsync($"standings?league={leagueId}&season={season}");
            if (doc == null) return GetMockStandings();

            var standings = new List<Standing>();
            try
            {
                var rows = doc.RootElement
                    .GetProperty("response")[0]
                    .GetProperty("league")
                    .GetProperty("standings")[0];

                foreach (var item in rows.EnumerateArray())
                {
                    var team = item.GetProperty("team");
                    var all = item.GetProperty("all");
                    standings.Add(new Standing
                    {
                        Rank = item.GetProperty("rank").GetInt32(),
                        TeamId = team.GetProperty("id").GetInt32(),
                        TeamName = team.GetProperty("name").GetString() ?? "",
                        TeamLogo = team.GetProperty("logo").GetString() ?? "",
                        Played = all.GetProperty("played").GetInt32(),
                        Won = all.GetProperty("win").GetInt32(),
                        Drawn = all.GetProperty("draw").GetInt32(),
                        Lost = all.GetProperty("lose").GetInt32(),
                        GoalsFor = all.GetProperty("goals").GetProperty("for").GetInt32(),
                        GoalsAgainst = all.GetProperty("goals").GetProperty("against").GetInt32(),
                        GoalDiff = item.GetProperty("goalsDiff").GetInt32(),
                        Points = item.GetProperty("points").GetInt32(),
                        Form = item.TryGetProperty("form", out var f) ? f.GetString() ?? "" : "",
                        Description = item.TryGetProperty("description", out var d) ? d.GetString() ?? "" : ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Parse error: {Error}", ex.Message);
                return GetMockStandings();
            }
            return standings;
        }

        public async Task<List<Player>> GetTopScorersAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTopScorers();

            var doc = await GetAsync($"players/topscorers?league={leagueId}&season={season}");
            if (doc == null) return GetMockTopScorers();

            var players = new List<Player>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var p = item.GetProperty("player");
                    var stats = item.GetProperty("statistics")[0];
                    var teamEl = stats.GetProperty("team");
                    var gamesEl = stats.GetProperty("games");
                    var goalsEl = stats.GetProperty("goals");

                    players.Add(new Player
                    {
                        Id = p.GetProperty("id").GetInt32(),
                        Name = p.GetProperty("name").GetString() ?? "",
                        Photo = p.GetProperty("photo").GetString() ?? "",
                        Nationality = p.GetProperty("nationality").GetString() ?? "",
                        Age = p.GetProperty("age").GetInt32(),
                        Position = gamesEl.TryGetProperty("position", out var pos) ? pos.GetString() ?? "" : "",
                        TeamName = teamEl.GetProperty("name").GetString() ?? "",
                        TeamLogo = teamEl.GetProperty("logo").GetString() ?? "",
                        Stats = new PlayerStats
                        {
                            Appearances = gamesEl.GetProperty("appearences").GetInt32(),
                            Goals = goalsEl.GetProperty("total").GetInt32(),
                            Assists = goalsEl.TryGetProperty("assists", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetInt32() : 0,
                            Rating = gamesEl.TryGetProperty("rating", out var r) && r.ValueKind != JsonValueKind.Null
                                ? double.TryParse(r.GetString(), out var rv) ? rv : 0 : 0
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Parse error: {Error}", ex.Message);
                return GetMockTopScorers();
            }
            return players;
        }

        public async Task<List<Match>> GetLiveMatchesAsync()
        {
            if (!HasApiKey) return GetMockLiveMatches();

            var doc = await GetAsync("fixtures?live=all");
            if (doc == null) return GetMockLiveMatches();

            return ParseMatches(doc, isLive: true);
        }

        public async Task<List<Match>> GetTodayMatchesAsync()
        {
            if (!HasApiKey) return GetMockTodayMatches();

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var doc = await GetAsync($"fixtures?date={today}");
            if (doc == null) return GetMockTodayMatches();

            return ParseMatches(doc);
        }

        public async Task<List<Team>> GetTeamsAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTeams();

            var doc = await GetAsync($"teams?league={leagueId}&season={season}");
            if (doc == null) return GetMockTeams();

            var teams = new List<Team>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var t = item.GetProperty("team");
                    var v = item.GetProperty("venue");
                    teams.Add(new Team
                    {
                        Id = t.GetProperty("id").GetInt32(),
                        Name = t.GetProperty("name").GetString() ?? "",
                        Logo = t.GetProperty("logo").GetString() ?? "",
                        Country = t.GetProperty("country").GetString() ?? "",
                        Founded = t.TryGetProperty("founded", out var f) && f.ValueKind != JsonValueKind.Null ? f.GetInt32() : 0,
                        Stadium = v.TryGetProperty("name", out var vn) && vn.ValueKind != JsonValueKind.Null ? vn.GetString() ?? "" : ""
                    });
                }
            }
            catch { return GetMockTeams(); }
            return teams;
        }

        public async Task<Team?> GetTeamDetailsAsync(int teamId, int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTeams().FirstOrDefault(t => t.Id == teamId);

            var doc = await GetAsync($"teams/statistics?team={teamId}&league={leagueId}&season={season}");
            if (doc == null) return null;

            try
            {
                var res = doc.RootElement.GetProperty("response");
                var teamEl = res.GetProperty("team");
                var fix = res.GetProperty("fixtures");

                return new Team
                {
                    Id = teamEl.GetProperty("id").GetInt32(),
                    Name = teamEl.GetProperty("name").GetString() ?? "",
                    Logo = teamEl.GetProperty("logo").GetString() ?? "",
                    Stats = new TeamStats
                    {
                        Played = fix.GetProperty("played").GetProperty("total").GetInt32(),
                        Won = fix.GetProperty("wins").GetProperty("total").GetInt32(),
                        Drawn = fix.GetProperty("draws").GetProperty("total").GetInt32(),
                        Lost = fix.GetProperty("loses").GetProperty("total").GetInt32(),
                        GoalsFor = res.GetProperty("goals").GetProperty("for").GetProperty("total").GetProperty("total").GetInt32(),
                        GoalsAgainst = res.GetProperty("goals").GetProperty("against").GetProperty("total").GetProperty("total").GetInt32()
                    }
                };
            }
            catch { return null; }
        }

        public async Task<Player?> GetPlayerDetailsAsync(int playerId, int season = 2024)
        {
            if (!HasApiKey) return GetMockTopScorers().FirstOrDefault(p => p.Id == playerId);

            var doc = await GetAsync($"players?id={playerId}&season={season}");
            if (doc == null) return null;

            try
            {
                var item = doc.RootElement.GetProperty("response")[0];
                var p = item.GetProperty("player");
                var stats = item.GetProperty("statistics")[0];
                var gamesEl = stats.GetProperty("games");
                var goalsEl = stats.GetProperty("goals");
                var passEl = stats.GetProperty("passes");
                var shotsEl = stats.GetProperty("shots");
                var cardsEl = stats.GetProperty("cards");

                return new Player
                {
                    Id = p.GetProperty("id").GetInt32(),
                    Name = p.GetProperty("name").GetString() ?? "",
                    Photo = p.GetProperty("photo").GetString() ?? "",
                    Nationality = p.GetProperty("nationality").GetString() ?? "",
                    Age = p.GetProperty("age").GetInt32(),
                    Position = gamesEl.TryGetProperty("position", out var pos) ? pos.GetString() ?? "" : "",
                    TeamName = stats.GetProperty("team").GetProperty("name").GetString() ?? "",
                    TeamLogo = stats.GetProperty("team").GetProperty("logo").GetString() ?? "",
                    Stats = new PlayerStats
                    {
                        Appearances = gamesEl.GetProperty("appearences").GetInt32(),
                        Goals = goalsEl.GetProperty("total").GetInt32(),
                        Assists = goalsEl.TryGetProperty("assists", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetInt32() : 0,
                        YellowCards = cardsEl.GetProperty("yellow").GetInt32(),
                        RedCards = cardsEl.GetProperty("red").GetInt32(),
                        Rating = gamesEl.TryGetProperty("rating", out var r) && r.ValueKind != JsonValueKind.Null
                            ? double.TryParse(r.GetString(), out var rv) ? rv : 0 : 0,
                        MinutesPlayed = gamesEl.TryGetProperty("minutes", out var m) && m.ValueKind != JsonValueKind.Null ? m.GetInt32() : 0,
                        Shots = shotsEl.TryGetProperty("total", out var st) && st.ValueKind != JsonValueKind.Null ? st.GetInt32() : 0,
                        ShotsOnTarget = shotsEl.TryGetProperty("on", out var so) && so.ValueKind != JsonValueKind.Null ? so.GetInt32() : 0,
                        Passes = passEl.TryGetProperty("total", out var pt) && pt.ValueKind != JsonValueKind.Null ? pt.GetInt32() : 0,
                        PassAccuracy = passEl.TryGetProperty("accuracy", out var pa) && pa.ValueKind != JsonValueKind.Null ? pa.GetInt32() : 0
                    }
                };
            }
            catch { return null; }
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var standings = await GetStandingsAsync();
            var topScorers = await GetTopScorersAsync();
            var liveMatches = await GetLiveMatchesAsync();
            var todayMatches = await GetTodayMatchesAsync();

            return new DashboardViewModel
            {
                LiveMatches = liveMatches,
                TodayMatches = todayMatches.Take(10).ToList(),
                TopStandings = standings.Take(5).ToList(),
                TopScorers = topScorers.Take(5).ToList(),
                LeagueName = "Premier League",
                LastUpdated = DateTime.Now
            };
        }

        private List<Match> ParseMatches(JsonDocument doc, bool isLive = false)
        {
            var matches = new List<Match>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var fixture = item.GetProperty("fixture");
                    var leagueEl = item.GetProperty("league");
                    var teamsEl = item.GetProperty("teams");
                    var goalsEl = item.GetProperty("goals");
                    var statusEl = fixture.GetProperty("status");

                    matches.Add(new Match
                    {
                        Id = fixture.GetProperty("id").GetInt32(),
                        HomeTeam = teamsEl.GetProperty("home").GetProperty("name").GetString() ?? "",
                        AwayTeam = teamsEl.GetProperty("away").GetProperty("name").GetString() ?? "",
                        HomeTeamLogo = teamsEl.GetProperty("home").GetProperty("logo").GetString() ?? "",
                        AwayTeamLogo = teamsEl.GetProperty("away").GetProperty("logo").GetString() ?? "",
                        HomeScore = goalsEl.GetProperty("home").ValueKind != JsonValueKind.Null ? goalsEl.GetProperty("home").GetInt32() : null,
                        AwayScore = goalsEl.GetProperty("away").ValueKind != JsonValueKind.Null ? goalsEl.GetProperty("away").GetInt32() : null,
                        Status = statusEl.GetProperty("short").GetString() ?? "",
                        Minute = statusEl.TryGetProperty("elapsed", out var el) && el.ValueKind != JsonValueKind.Null ? el.GetInt32() : null,
                        LeagueName = leagueEl.GetProperty("name").GetString() ?? "",
                        LeagueLogo = leagueEl.GetProperty("logo").GetString() ?? "",
                        Date = DateTime.TryParse(fixture.GetProperty("date").GetString(), out var dt) ? dt : DateTime.Now
                    });
                }
            }
            catch { }
            return matches;
        }

        // ─── MOCK DATA (used when no API key is configured) ───────────────────

        private List<Standing> GetMockStandings() => new()
        {
            new Standing { Rank=1, TeamId=33, TeamName="Manchester City", TeamLogo="https://media.api-sports.io/football/teams/50.png", Played=28, Won=21, Drawn=4, Lost=3, GoalsFor=62, GoalsAgainst=28, GoalDiff=34, Points=67, Form="WWWDW", Description="Champions League" },
            new Standing { Rank=2, TeamId=40, TeamName="Liverpool", TeamLogo="https://media.api-sports.io/football/teams/40.png", Played=28, Won=20, Drawn=5, Lost=3, GoalsFor=70, GoalsAgainst=30, GoalDiff=40, Points=65, Form="WWWWW", Description="Champions League" },
            new Standing { Rank=3, TeamId=42, TeamName="Arsenal", TeamLogo="https://media.api-sports.io/football/teams/42.png", Played=28, Won=18, Drawn=5, Lost=5, GoalsFor=65, GoalsAgainst=32, GoalDiff=33, Points=59, Form="WWDWW", Description="Champions League" },
            new Standing { Rank=4, TeamId=66, TeamName="Aston Villa", TeamLogo="https://media.api-sports.io/football/teams/66.png", Played=28, Won=17, Drawn=4, Lost=7, GoalsFor=68, GoalsAgainst=52, GoalDiff=16, Points=55, Form="WDWWL", Description="Champions League" },
            new Standing { Rank=5, TeamId=49, TeamName="Chelsea", TeamLogo="https://media.api-sports.io/football/teams/49.png", Played=28, Won=14, Drawn=5, Lost=9, GoalsFor=60, GoalsAgainst=46, GoalDiff=14, Points=47, Form="WLWWD", Description="Europa League" },
            new Standing { Rank=6, TeamId=34, TeamName="Newcastle", TeamLogo="https://media.api-sports.io/football/teams/34.png", Played=28, Won=12, Drawn=7, Lost=9, GoalsFor=56, GoalsAgainst=42, GoalDiff=14, Points=43, Form="DWWLW", Description="Europa League" },
            new Standing { Rank=7, TeamId=35, TeamName="Manchester United", TeamLogo="https://media.api-sports.io/football/teams/33.png", Played=28, Won=11, Drawn=4, Lost=13, GoalsFor=30, GoalsAgainst=44, GoalDiff=-14, Points=37, Form="LWLDL" },
            new Standing { Rank=8, TeamId=47, TeamName="Tottenham", TeamLogo="https://media.api-sports.io/football/teams/47.png", Played=28, Won=10, Drawn=6, Lost=12, GoalsFor=55, GoalsAgainst=56, GoalDiff=-1, Points=36, Form="WLLWL" },
            new Standing { Rank=9, TeamId=55, TeamName="West Ham", TeamLogo="https://media.api-sports.io/football/teams/55.png", Played=28, Won=10, Drawn=5, Lost=13, GoalsFor=44, GoalsAgainst=54, GoalDiff=-10, Points=35, Form="LWLWL" },
            new Standing { Rank=10, TeamId=51, TeamName="Brighton", TeamLogo="https://media.api-sports.io/football/teams/51.png", Played=28, Won=9, Drawn=7, Lost=12, GoalsFor=50, GoalsAgainst=54, GoalDiff=-4, Points=34, Form="DWLLD" },
            new Standing { Rank=11, TeamId=36, TeamName="Fulham", TeamLogo="https://media.api-sports.io/football/teams/36.png", Played=28, Won=9, Drawn=6, Lost=13, GoalsFor=44, GoalsAgainst=52, GoalDiff=-8, Points=33, Form="LLLWW" },
            new Standing { Rank=12, TeamId=52, TeamName="Crystal Palace", TeamLogo="https://media.api-sports.io/football/teams/52.png", Played=28, Won=7, Drawn=9, Lost=12, GoalsFor=34, GoalsAgainst=49, GoalDiff=-15, Points=30, Form="DLDWL" },
            new Standing { Rank=13, TeamId=45, TeamName="Everton", TeamLogo="https://media.api-sports.io/football/teams/45.png", Played=28, Won=7, Drawn=7, Lost=14, GoalsFor=32, GoalsAgainst=48, GoalDiff=-16, Points=28, Form="LDLLL" },
            new Standing { Rank=14, TeamId=39, TeamName="Wolves", TeamLogo="https://media.api-sports.io/football/teams/39.png", Played=28, Won=6, Drawn=7, Lost=15, GoalsFor=36, GoalsAgainst=57, GoalDiff=-21, Points=25, Form="LLLWL" },
            new Standing { Rank=15, TeamId=48, TeamName="Brentford", TeamLogo="https://media.api-sports.io/football/teams/48.png", Played=28, Won=6, Drawn=6, Lost=16, GoalsFor=40, GoalsAgainst=58, GoalDiff=-18, Points=24, Form="WLDLL" },
            new Standing { Rank=16, TeamId=41, TeamName="Nottm Forest", TeamLogo="https://media.api-sports.io/football/teams/65.png", Played=28, Won=5, Drawn=7, Lost=16, GoalsFor=30, GoalsAgainst=56, GoalDiff=-26, Points=22, Form="LLDLL" },
            new Standing { Rank=17, TeamId=46, TeamName="Bournemouth", TeamLogo="https://media.api-sports.io/football/teams/35.png", Played=28, Won=5, Drawn=6, Lost=17, GoalsFor=34, GoalsAgainst=62, GoalDiff=-28, Points=21, Form="LLLLL", Description="Relegation" },
            new Standing { Rank=18, TeamId=57, TeamName="Sheffield United", TeamLogo="https://media.api-sports.io/football/teams/62.png", Played=28, Won=3, Drawn=6, Lost=19, GoalsFor=28, GoalsAgainst=82, GoalDiff=-54, Points=15, Form="LLLLL", Description="Relegation" },
            new Standing { Rank=19, TeamId=44, TeamName="Burnley", TeamLogo="https://media.api-sports.io/football/teams/44.png", Played=28, Won=4, Drawn=3, Lost=21, GoalsFor=28, GoalsAgainst=66, GoalDiff=-38, Points=15, Form="LLLLL", Description="Relegation" },
            new Standing { Rank=20, TeamId=63, TeamName="Luton", TeamLogo="https://media.api-sports.io/football/teams/1359.png", Played=28, Won=3, Drawn=5, Lost=20, GoalsFor=36, GoalsAgainst=70, GoalDiff=-34, Points=14, Form="LLLLL", Description="Relegation" }
        };

        private List<Player> GetMockTopScorers() => new()
        {
            new Player { Id=1, Name="Erling Haaland", Photo="https://media.api-sports.io/football/players/1100.png", Nationality="Norway", Age=23, Position="Attacker", TeamName="Manchester City", TeamLogo="https://media.api-sports.io/football/teams/50.png", Stats=new PlayerStats { Appearances=27, Goals=20, Assists=5, Rating=7.85, YellowCards=1, RedCards=0, MinutesPlayed=2260, Shots=95, ShotsOnTarget=55, Passes=520, PassAccuracy=74 } },
            new Player { Id=2, Name="Cole Palmer", Photo="https://media.api-sports.io/football/players/284646.png", Nationality="England", Age=21, Position="Midfielder", TeamName="Chelsea", TeamLogo="https://media.api-sports.io/football/teams/49.png", Stats=new PlayerStats { Appearances=26, Goals=18, Assists=10, Rating=7.92, YellowCards=3, RedCards=0, MinutesPlayed=2200, Shots=80, ShotsOnTarget=48, Passes=900, PassAccuracy=82 } },
            new Player { Id=3, Name="Alexander Isak", Photo="https://media.api-sports.io/football/players/169.png", Nationality="Sweden", Age=24, Position="Attacker", TeamName="Newcastle", TeamLogo="https://media.api-sports.io/football/teams/34.png", Stats=new PlayerStats { Appearances=25, Goals=17, Assists=3, Rating=7.60, YellowCards=2, RedCards=0, MinutesPlayed=2100, Shots=78, ShotsOnTarget=42, Passes=440, PassAccuracy=72 } },
            new Player { Id=4, Name="Bukayo Saka", Photo="https://media.api-sports.io/football/players/184.png", Nationality="England", Age=22, Position="Attacker", TeamName="Arsenal", TeamLogo="https://media.api-sports.io/football/teams/42.png", Stats=new PlayerStats { Appearances=27, Goals=15, Assists=10, Rating=7.80, YellowCards=2, RedCards=0, MinutesPlayed=2380, Shots=70, ShotsOnTarget=40, Passes=1100, PassAccuracy=85 } },
            new Player { Id=5, Name="Mohamed Salah", Photo="https://media.api-sports.io/football/players/306.png", Nationality="Egypt", Age=31, Position="Attacker", TeamName="Liverpool", TeamLogo="https://media.api-sports.io/football/teams/40.png", Stats=new PlayerStats { Appearances=27, Goals=17, Assists=12, Rating=8.10, YellowCards=0, RedCards=0, MinutesPlayed=2350, Shots=85, ShotsOnTarget=50, Passes=850, PassAccuracy=81 } },
            new Player { Id=6, Name="Ollie Watkins", Photo="https://media.api-sports.io/football/players/181.png", Nationality="England", Age=28, Position="Attacker", TeamName="Aston Villa", TeamLogo="https://media.api-sports.io/football/teams/66.png", Stats=new PlayerStats { Appearances=28, Goals=16, Assists=11, Rating=7.75, YellowCards=3, RedCards=0, MinutesPlayed=2430, Shots=75, ShotsOnTarget=38, Passes=600, PassAccuracy=71 } },
            new Player { Id=7, Name="Jarrod Bowen", Photo="https://media.api-sports.io/football/players/200.png", Nationality="England", Age=27, Position="Attacker", TeamName="West Ham", TeamLogo="https://media.api-sports.io/football/teams/55.png", Stats=new PlayerStats { Appearances=26, Goals=14, Assists=6, Rating=7.30, YellowCards=4, RedCards=0, MinutesPlayed=2200, Shots=65, ShotsOnTarget=35, Passes=700, PassAccuracy=76 } },
            new Player { Id=8, Name="Son Heung-min", Photo="https://media.api-sports.io/football/players/521.png", Nationality="South Korea", Age=31, Position="Attacker", TeamName="Tottenham", TeamLogo="https://media.api-sports.io/football/teams/47.png", Stats=new PlayerStats { Appearances=27, Goals=13, Assists=7, Rating=7.20, YellowCards=1, RedCards=0, MinutesPlayed=2310, Shots=68, ShotsOnTarget=35, Passes=780, PassAccuracy=79 } },
            new Player { Id=9, Name="Dominic Solanke", Photo="https://media.api-sports.io/football/players/18.png", Nationality="England", Age=26, Position="Attacker", TeamName="Bournemouth", TeamLogo="https://media.api-sports.io/football/teams/35.png", Stats=new PlayerStats { Appearances=28, Goals=13, Assists=4, Rating=7.00, YellowCards=3, RedCards=0, MinutesPlayed=2460, Shots=60, ShotsOnTarget=30, Passes=550, PassAccuracy=68 } },
            new Player { Id=10, Name="Leandro Trossard", Photo="https://media.api-sports.io/football/players/747.png", Nationality="Belgium", Age=29, Position="Midfielder", TeamName="Arsenal", TeamLogo="https://media.api-sports.io/football/teams/42.png", Stats=new PlayerStats { Appearances=25, Goals=10, Assists=5, Rating=7.10, YellowCards=2, RedCards=0, MinutesPlayed=1900, Shots=55, ShotsOnTarget=28, Passes=870, PassAccuracy=84 } },
        };

        private List<Match> GetMockLiveMatches() => new()
        {
            new Match { Id=1, HomeTeam="Liverpool", AwayTeam="Arsenal", HomeTeamLogo="https://media.api-sports.io/football/teams/40.png", AwayTeamLogo="https://media.api-sports.io/football/teams/42.png", HomeScore=2, AwayScore=1, Status="1H", Minute=38, LeagueName="Premier League", LeagueLogo="https://media.api-sports.io/football/leagues/39.png", Date=DateTime.Now },
            new Match { Id=2, HomeTeam="Real Madrid", AwayTeam="Barcelona", HomeTeamLogo="https://media.api-sports.io/football/teams/541.png", AwayTeamLogo="https://media.api-sports.io/football/teams/529.png", HomeScore=1, AwayScore=1, Status="2H", Minute=67, LeagueName="La Liga", LeagueLogo="https://media.api-sports.io/football/leagues/140.png", Date=DateTime.Now },
        };

        private List<Match> GetMockTodayMatches() => new()
        {
            new Match { Id=1, HomeTeam="Liverpool", AwayTeam="Arsenal", HomeTeamLogo="https://media.api-sports.io/football/teams/40.png", AwayTeamLogo="https://media.api-sports.io/football/teams/42.png", HomeScore=2, AwayScore=1, Status="1H", Minute=38, LeagueName="Premier League", LeagueLogo="https://media.api-sports.io/football/leagues/39.png", Date=DateTime.Now.AddHours(-1) },
            new Match { Id=2, HomeTeam="Real Madrid", AwayTeam="Barcelona", HomeTeamLogo="https://media.api-sports.io/football/teams/541.png", AwayTeamLogo="https://media.api-sports.io/football/teams/529.png", HomeScore=1, AwayScore=1, Status="2H", Minute=67, LeagueName="La Liga", LeagueLogo="https://media.api-sports.io/football/leagues/140.png", Date=DateTime.Now.AddHours(-1) },
            new Match { Id=3, HomeTeam="Bayern Munich", AwayTeam="Dortmund", HomeTeamLogo="https://media.api-sports.io/football/teams/157.png", AwayTeamLogo="https://media.api-sports.io/football/teams/165.png", HomeScore=null, AwayScore=null, Status="NS", LeagueName="Bundesliga", LeagueLogo="https://media.api-sports.io/football/leagues/78.png", Date=DateTime.Now.AddHours(2) },
            new Match { Id=4, HomeTeam="PSG", AwayTeam="Lyon", HomeTeamLogo="https://media.api-sports.io/football/teams/85.png", AwayTeamLogo="https://media.api-sports.io/football/teams/80.png", HomeScore=null, AwayScore=null, Status="NS", LeagueName="Ligue 1", LeagueLogo="https://media.api-sports.io/football/leagues/61.png", Date=DateTime.Now.AddHours(3) },
            new Match { Id=5, HomeTeam="Manchester City", AwayTeam="Chelsea", HomeTeamLogo="https://media.api-sports.io/football/teams/50.png", AwayTeamLogo="https://media.api-sports.io/football/teams/49.png", HomeScore=3, AwayScore=0, Status="FT", LeagueName="Premier League", LeagueLogo="https://media.api-sports.io/football/leagues/39.png", Date=DateTime.Now.AddHours(-3) },
        };

        private List<Team> GetMockTeams() => new()
        {
            new Team { Id=33, Name="Manchester City", Logo="https://media.api-sports.io/football/teams/50.png", Country="England", Founded=1880, Stadium="Etihad Stadium", Stats=new TeamStats { Played=28, Won=21, Drawn=4, Lost=3, GoalsFor=62, GoalsAgainst=28 } },
            new Team { Id=40, Name="Liverpool", Logo="https://media.api-sports.io/football/teams/40.png", Country="England", Founded=1892, Stadium="Anfield", Stats=new TeamStats { Played=28, Won=20, Drawn=5, Lost=3, GoalsFor=70, GoalsAgainst=30 } },
            new Team { Id=42, Name="Arsenal", Logo="https://media.api-sports.io/football/teams/42.png", Country="England", Founded=1886, Stadium="Emirates Stadium", Stats=new TeamStats { Played=28, Won=18, Drawn=5, Lost=5, GoalsFor=65, GoalsAgainst=32 } },
            new Team { Id=66, Name="Aston Villa", Logo="https://media.api-sports.io/football/teams/66.png", Country="England", Founded=1874, Stadium="Villa Park", Stats=new TeamStats { Played=28, Won=17, Drawn=4, Lost=7, GoalsFor=68, GoalsAgainst=52 } },
            new Team { Id=49, Name="Chelsea", Logo="https://media.api-sports.io/football/teams/49.png", Country="England", Founded=1905, Stadium="Stamford Bridge", Stats=new TeamStats { Played=28, Won=14, Drawn=5, Lost=9, GoalsFor=60, GoalsAgainst=46 } },
            new Team { Id=34, Name="Newcastle", Logo="https://media.api-sports.io/football/teams/34.png", Country="England", Founded=1892, Stadium="St. James' Park", Stats=new TeamStats { Played=28, Won=12, Drawn=7, Lost=9, GoalsFor=56, GoalsAgainst=42 } },
        };
    }
}

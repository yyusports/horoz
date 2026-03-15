using System.Text.Json;
using Sportify.Models;

namespace Sportify.Services
{
    public interface ISportDataService
    {
        Task<List<PuanDurumu>> GetStandingsAsync(int leagueId, int season);
        Task<List<Oyuncu>> GetTopScorersAsync(int leagueId, int season);
        Task<List<Mac>> GetLiveMatchesAsync();
        Task<List<Mac>> GetTodayMatchesAsync();
        Task<List<Takim>> GetTeamsAsync(int leagueId, int season);
        Task<Takim?> GetTeamDetailsAsync(int teamId, int leagueId, int season);
        Task<Oyuncu?> GetPlayerDetailsAsync(int playerId, int season);
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

        public async Task<List<PuanDurumu>> GetStandingsAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockStandings();

            var doc = await GetAsync($"standings?league={leagueId}&season={season}");
            if (doc == null) return GetMockStandings();

            var standings = new List<PuanDurumu>();
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
                    standings.Add(new PuanDurumu
                    {
                        Sira = item.GetProperty("rank").GetInt32(),
                        TakimId = team.GetProperty("id").GetInt32(),
                        TakimAdi = team.GetProperty("name").GetString() ?? "",
                        TakimLogo = team.GetProperty("logo").GetString() ?? "",
                        Oynanan = all.GetProperty("played").GetInt32(),
                        Galibiyet = all.GetProperty("win").GetInt32(),
                        Beraberlik = all.GetProperty("draw").GetInt32(),
                        Maglubiyet = all.GetProperty("lose").GetInt32(),
                        AtilanGol = all.GetProperty("goals").GetProperty("for").GetInt32(),
                        YenilenGol = all.GetProperty("goals").GetProperty("against").GetInt32(),
                        GolFarki = item.GetProperty("goalsDiff").GetInt32(),
                        Puan = item.GetProperty("points").GetInt32(),
                        Form = item.TryGetProperty("form", out var f) ? f.GetString() ?? "" : "",
                        Aciklama = item.TryGetProperty("description", out var d) ? d.GetString() ?? "" : ""
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

        public async Task<List<Oyuncu>> GetTopScorersAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTopScorers();

            var doc = await GetAsync($"players/topscorers?league={leagueId}&season={season}");
            if (doc == null) return GetMockTopScorers();

            var players = new List<Oyuncu>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var p = item.GetProperty("player");
                    var stats = item.GetProperty("statistics")[0];
                    var teamEl = stats.GetProperty("team");
                    var gamesEl = stats.GetProperty("games");
                    var goalsEl = stats.GetProperty("goals");

                    players.Add(new Oyuncu
                    {
                        Id = p.GetProperty("id").GetInt32(),
                        Ad = p.GetProperty("name").GetString() ?? "",
                        Foto = p.GetProperty("photo").GetString() ?? "",
                        Uyruk = p.GetProperty("nationality").GetString() ?? "",
                        Yas = p.GetProperty("age").GetInt32(),
                        Mevki = gamesEl.TryGetProperty("position", out var pos) ? pos.GetString() ?? "" : "",
                        TakimAdi = teamEl.GetProperty("name").GetString() ?? "",
                        TakimLogo = teamEl.GetProperty("logo").GetString() ?? "",
                        Istatistikler = new OyuncuIstatistikleri
                        {
                            MacSayisi = gamesEl.GetProperty("appearences").GetInt32(),
                            Goller = goalsEl.GetProperty("total").GetInt32(),
                            Asistler = goalsEl.TryGetProperty("assists", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetInt32() : 0,
                            Reyting = gamesEl.TryGetProperty("rating", out var r) && r.ValueKind != JsonValueKind.Null
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

        public async Task<List<Mac>> GetLiveMatchesAsync()
        {
            if (!HasApiKey) return GetMockLiveMatches();

            var doc = await GetAsync("fixtures?live=all");
            if (doc == null) return GetMockLiveMatches();

            return ParseMatches(doc, isLive: true);
        }

        public async Task<List<Mac>> GetTodayMatchesAsync()
        {
            if (!HasApiKey) return GetMockTodayMatches();

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var doc = await GetAsync($"fixtures?date={today}");
            if (doc == null) return GetMockTodayMatches();

            return ParseMatches(doc);
        }

        public async Task<List<Takim>> GetTeamsAsync(int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTeams();

            var doc = await GetAsync($"teams?league={leagueId}&season={season}");
            if (doc == null) return GetMockTeams();

            var teams = new List<Takim>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var t = item.GetProperty("team");
                    var v = item.GetProperty("venue");
                    teams.Add(new Takim
                    {
                        Id = t.GetProperty("id").GetInt32(),
                        Ad = t.GetProperty("name").GetString() ?? "",
                        Logo = t.GetProperty("logo").GetString() ?? "",
                        Ulke = t.GetProperty("country").GetString() ?? "",
                        KurulusYili = t.TryGetProperty("founded", out var f) && f.ValueKind != JsonValueKind.Null ? f.GetInt32() : 0,
                        Stadyum = v.TryGetProperty("name", out var vn) && vn.ValueKind != JsonValueKind.Null ? vn.GetString() ?? "" : ""
                    });
                }
            }
            catch { return GetMockTeams(); }
            return teams;
        }

        public async Task<Takim?> GetTeamDetailsAsync(int teamId, int leagueId = 39, int season = 2024)
        {
            if (!HasApiKey) return GetMockTeams().FirstOrDefault(t => t.Id == teamId);

            var doc = await GetAsync($"teams/statistics?team={teamId}&league={leagueId}&season={season}");
            if (doc == null) return null;

            try
            {
                var res = doc.RootElement.GetProperty("response");
                var teamEl = res.GetProperty("team");
                var fix = res.GetProperty("fixtures");

                return new Takim
                {
                    Id = teamEl.GetProperty("id").GetInt32(),
                    Ad = teamEl.GetProperty("name").GetString() ?? "",
                    Logo = teamEl.GetProperty("logo").GetString() ?? "",
                    Istatistikler = new TakimIstatistikleri
                    {
                        Oynanan = fix.GetProperty("played").GetProperty("total").GetInt32(),
                        Galibiyet = fix.GetProperty("wins").GetProperty("total").GetInt32(),
                        Beraberlik = fix.GetProperty("draws").GetProperty("total").GetInt32(),
                        Maglubiyet = fix.GetProperty("loses").GetProperty("total").GetInt32(),
                        AtilanGol = res.GetProperty("goals").GetProperty("for").GetProperty("total").GetProperty("total").GetInt32(),
                        YenilenGol = res.GetProperty("goals").GetProperty("against").GetProperty("total").GetProperty("total").GetInt32()
                    }
                };
            }
            catch { return null; }
        }

        public async Task<Oyuncu?> GetPlayerDetailsAsync(int playerId, int season = 2024)
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

                return new Oyuncu
                {
                    Id = p.GetProperty("id").GetInt32(),
                    Ad = p.GetProperty("name").GetString() ?? "",
                    Foto = p.GetProperty("photo").GetString() ?? "",
                    Uyruk = p.GetProperty("nationality").GetString() ?? "",
                    Yas = p.GetProperty("age").GetInt32(),
                    Mevki = gamesEl.TryGetProperty("position", out var pos) ? pos.GetString() ?? "" : "",
                    TakimAdi = stats.GetProperty("team").GetProperty("name").GetString() ?? "",
                    TakimLogo = stats.GetProperty("team").GetProperty("logo").GetString() ?? "",
                    Istatistikler = new OyuncuIstatistikleri
                    {
                        MacSayisi = gamesEl.GetProperty("appearences").GetInt32(),
                        Goller = goalsEl.GetProperty("total").GetInt32(),
                        Asistler = goalsEl.TryGetProperty("assists", out var a) && a.ValueKind != JsonValueKind.Null ? a.GetInt32() : 0,
                        SariKartlar = cardsEl.GetProperty("yellow").GetInt32(),
                        KirmiziKartlar = cardsEl.GetProperty("red").GetInt32(),
                        Reyting = gamesEl.TryGetProperty("rating", out var r) && r.ValueKind != JsonValueKind.Null
                            ? double.TryParse(r.GetString(), out var rv) ? rv : 0 : 0,
                        OynananDakika = gamesEl.TryGetProperty("minutes", out var m) && m.ValueKind != JsonValueKind.Null ? m.GetInt32() : 0,
                        Sutlar = shotsEl.TryGetProperty("total", out var st) && st.ValueKind != JsonValueKind.Null ? st.GetInt32() : 0,
                        IsabetliSutlar = shotsEl.TryGetProperty("on", out var so) && so.ValueKind != JsonValueKind.Null ? so.GetInt32() : 0,
                        Paslar = passEl.TryGetProperty("total", out var pt) && pt.ValueKind != JsonValueKind.Null ? pt.GetInt32() : 0,
                        PasIsabeti = passEl.TryGetProperty("accuracy", out var pa) && pa.ValueKind != JsonValueKind.Null ? pa.GetInt32() : 0
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
                CanliMaclar = liveMatches,
                BugunMaclar = todayMatches.Take(10).ToList(),
                UstSiraPuanDurumu = standings.Take(5).ToList(),
                GolKralligi = topScorers.Take(5).ToList(),
                LigAdi = "Premier League",
                SonGuncelleme = DateTime.Now
            };
        }

        private List<Mac> ParseMatches(JsonDocument doc, bool isLive = false)
        {
            var matches = new List<Mac>();
            try
            {
                foreach (var item in doc.RootElement.GetProperty("response").EnumerateArray())
                {
                    var fixture = item.GetProperty("fixture");
                    var leagueEl = item.GetProperty("league");
                    var teamsEl = item.GetProperty("teams");
                    var goalsEl = item.GetProperty("goals");
                    var statusEl = fixture.GetProperty("status");

                    matches.Add(new Mac
                    {
                        Id = fixture.GetProperty("id").GetInt32(),
                        EvSahibi = teamsEl.GetProperty("home").GetProperty("name").GetString() ?? "",
                        Deplasman = teamsEl.GetProperty("away").GetProperty("name").GetString() ?? "",
                        EvSahibiLogo = teamsEl.GetProperty("home").GetProperty("logo").GetString() ?? "",
                        DeplasmanLogo = teamsEl.GetProperty("away").GetProperty("logo").GetString() ?? "",
                        EvSahibiSkor = goalsEl.GetProperty("home").ValueKind != JsonValueKind.Null ? goalsEl.GetProperty("home").GetInt32() : null,
                        DeplasmanSkor = goalsEl.GetProperty("away").ValueKind != JsonValueKind.Null ? goalsEl.GetProperty("away").GetInt32() : null,
                        Durum = statusEl.GetProperty("short").GetString() ?? "",
                        Dakika = statusEl.TryGetProperty("elapsed", out var el) && el.ValueKind != JsonValueKind.Null ? el.GetInt32() : null,
                        LigAdi = leagueEl.GetProperty("name").GetString() ?? "",
                        LigLogo = leagueEl.GetProperty("logo").GetString() ?? "",
                        Tarih = DateTime.TryParse(fixture.GetProperty("date").GetString(), out var dt) ? dt : DateTime.Now
                    });
                }
            }
            catch { }
            return matches;
        }

        // ─── MOCK DATA (used when no API key is configured) ───────────────────

        private List<PuanDurumu> GetMockStandings() => new()
        {
            new PuanDurumu { Sira=1, TakimId=33, TakimAdi="Manchester City", TakimLogo="https://media.api-sports.io/football/teams/50.png", Oynanan=28, Galibiyet=21, Beraberlik=4, Maglubiyet=3, AtilanGol=62, YenilenGol=28, GolFarki=34, Puan=67, Form="WWWDW", Aciklama="Champions League" },
            new PuanDurumu { Sira=2, TakimId=40, TakimAdi="Liverpool", TakimLogo="https://media.api-sports.io/football/teams/40.png", Oynanan=28, Galibiyet=20, Beraberlik=5, Maglubiyet=3, AtilanGol=70, YenilenGol=30, GolFarki=40, Puan=65, Form="WWWWW", Aciklama="Champions League" },
            new PuanDurumu { Sira=3, TakimId=42, TakimAdi="Arsenal", TakimLogo="https://media.api-sports.io/football/teams/42.png", Oynanan=28, Galibiyet=18, Beraberlik=5, Maglubiyet=5, AtilanGol=65, YenilenGol=32, GolFarki=33, Puan=59, Form="WWDWW", Aciklama="Champions League" },
            new PuanDurumu { Sira=4, TakimId=66, TakimAdi="Aston Villa", TakimLogo="https://media.api-sports.io/football/teams/66.png", Oynanan=28, Galibiyet=17, Beraberlik=4, Maglubiyet=7, AtilanGol=68, YenilenGol=52, GolFarki=16, Puan=55, Form="WDWWL", Aciklama="Champions League" },
            new PuanDurumu { Sira=5, TakimId=49, TakimAdi="Chelsea", TakimLogo="https://media.api-sports.io/football/teams/49.png", Oynanan=28, Galibiyet=14, Beraberlik=5, Maglubiyet=9, AtilanGol=60, YenilenGol=46, GolFarki=14, Puan=47, Form="WLWWD", Aciklama="Europa League" },
            new PuanDurumu { Sira=6, TakimId=34, TakimAdi="Newcastle", TakimLogo="https://media.api-sports.io/football/teams/34.png", Oynanan=28, Galibiyet=12, Beraberlik=7, Maglubiyet=9, AtilanGol=56, YenilenGol=42, GolFarki=14, Puan=43, Form="DWWLW", Aciklama="Europa League" },
            new PuanDurumu { Sira=7, TakimId=35, TakimAdi="Manchester United", TakimLogo="https://media.api-sports.io/football/teams/33.png", Oynanan=28, Galibiyet=11, Beraberlik=4, Maglubiyet=13, AtilanGol=30, YenilenGol=44, GolFarki=-14, Puan=37, Form="LWLDL" },
            new PuanDurumu { Sira=8, TakimId=47, TakimAdi="Tottenham", TakimLogo="https://media.api-sports.io/football/teams/47.png", Oynanan=28, Galibiyet=10, Beraberlik=6, Maglubiyet=12, AtilanGol=55, YenilenGol=56, GolFarki=-1, Puan=36, Form="WLLWL" },
            new PuanDurumu { Sira=9, TakimId=55, TakimAdi="West Ham", TakimLogo="https://media.api-sports.io/football/teams/55.png", Oynanan=28, Galibiyet=10, Beraberlik=5, Maglubiyet=13, AtilanGol=44, YenilenGol=54, GolFarki=-10, Puan=35, Form="LWLWL" },
            new PuanDurumu { Sira=10, TakimId=51, TakimAdi="Brighton", TakimLogo="https://media.api-sports.io/football/teams/51.png", Oynanan=28, Galibiyet=9, Beraberlik=7, Maglubiyet=12, AtilanGol=50, YenilenGol=54, GolFarki=-4, Puan=34, Form="DWLLD" },
            new PuanDurumu { Sira=11, TakimId=36, TakimAdi="Fulham", TakimLogo="https://media.api-sports.io/football/teams/36.png", Oynanan=28, Galibiyet=9, Beraberlik=6, Maglubiyet=13, AtilanGol=44, YenilenGol=52, GolFarki=-8, Puan=33, Form="LLLWW" },
            new PuanDurumu { Sira=12, TakimId=52, TakimAdi="Crystal Palace", TakimLogo="https://media.api-sports.io/football/teams/52.png", Oynanan=28, Galibiyet=7, Beraberlik=9, Maglubiyet=12, AtilanGol=34, YenilenGol=49, GolFarki=-15, Puan=30, Form="DLDWL" },
            new PuanDurumu { Sira=13, TakimId=45, TakimAdi="Everton", TakimLogo="https://media.api-sports.io/football/teams/45.png", Oynanan=28, Galibiyet=7, Beraberlik=7, Maglubiyet=14, AtilanGol=32, YenilenGol=48, GolFarki=-16, Puan=28, Form="LDLLL" },
            new PuanDurumu { Sira=14, TakimId=39, TakimAdi="Wolves", TakimLogo="https://media.api-sports.io/football/teams/39.png", Oynanan=28, Galibiyet=6, Beraberlik=7, Maglubiyet=15, AtilanGol=36, YenilenGol=57, GolFarki=-21, Puan=25, Form="LLLWL" },
            new PuanDurumu { Sira=15, TakimId=48, TakimAdi="Brentford", TakimLogo="https://media.api-sports.io/football/teams/48.png", Oynanan=28, Galibiyet=6, Beraberlik=6, Maglubiyet=16, AtilanGol=40, YenilenGol=58, GolFarki=-18, Puan=24, Form="WLDLL" },
            new PuanDurumu { Sira=16, TakimId=41, TakimAdi="Nottm Forest", TakimLogo="https://media.api-sports.io/football/teams/65.png", Oynanan=28, Galibiyet=5, Beraberlik=7, Maglubiyet=16, AtilanGol=30, YenilenGol=56, GolFarki=-26, Puan=22, Form="LLDLL" },
            new PuanDurumu { Sira=17, TakimId=46, TakimAdi="Bournemouth", TakimLogo="https://media.api-sports.io/football/teams/35.png", Oynanan=28, Galibiyet=5, Beraberlik=6, Maglubiyet=17, AtilanGol=34, YenilenGol=62, GolFarki=-28, Puan=21, Form="LLLLL", Aciklama="Relegation" },
            new PuanDurumu { Sira=18, TakimId=57, TakimAdi="Sheffield United", TakimLogo="https://media.api-sports.io/football/teams/62.png", Oynanan=28, Galibiyet=3, Beraberlik=6, Maglubiyet=19, AtilanGol=28, YenilenGol=82, GolFarki=-54, Puan=15, Form="LLLLL", Aciklama="Relegation" },
            new PuanDurumu { Sira=19, TakimId=44, TakimAdi="Burnley", TakimLogo="https://media.api-sports.io/football/teams/44.png", Oynanan=28, Galibiyet=4, Beraberlik=3, Maglubiyet=21, AtilanGol=28, YenilenGol=66, GolFarki=-38, Puan=15, Form="LLLLL", Aciklama="Relegation" },
            new PuanDurumu { Sira=20, TakimId=63, TakimAdi="Luton", TakimLogo="https://media.api-sports.io/football/teams/1359.png", Oynanan=28, Galibiyet=3, Beraberlik=5, Maglubiyet=20, AtilanGol=36, YenilenGol=70, GolFarki=-34, Puan=14, Form="LLLLL", Aciklama="Relegation" }
        };

        private List<Oyuncu> GetMockTopScorers() => new()
        {
            new Oyuncu { Id=1, Ad="Erling Haaland", Foto="https://media.api-sports.io/football/players/1100.png", Uyruk="Norway", Yas=23, Mevki="Attacker", TakimAdi="Manchester City", TakimLogo="https://media.api-sports.io/football/teams/50.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=27, Goller=20, Asistler=5, Reyting=7.85, SariKartlar=1, KirmiziKartlar=0, OynananDakika=2260, Sutlar=95, IsabetliSutlar=55, Paslar=520, PasIsabeti=74 } },
            new Oyuncu { Id=2, Ad="Cole Palmer", Foto="https://media.api-sports.io/football/players/284646.png", Uyruk="England", Yas=21, Mevki="Midfielder", TakimAdi="Chelsea", TakimLogo="https://media.api-sports.io/football/teams/49.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=26, Goller=18, Asistler=10, Reyting=7.92, SariKartlar=3, KirmiziKartlar=0, OynananDakika=2200, Sutlar=80, IsabetliSutlar=48, Paslar=900, PasIsabeti=82 } },
            new Oyuncu { Id=3, Ad="Alexander Isak", Foto="https://media.api-sports.io/football/players/169.png", Uyruk="Sweden", Yas=24, Mevki="Attacker", TakimAdi="Newcastle", TakimLogo="https://media.api-sports.io/football/teams/34.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=25, Goller=17, Asistler=3, Reyting=7.60, SariKartlar=2, KirmiziKartlar=0, OynananDakika=2100, Sutlar=78, IsabetliSutlar=42, Paslar=440, PasIsabeti=72 } },
            new Oyuncu { Id=4, Ad="Bukayo Saka", Foto="https://media.api-sports.io/football/players/184.png", Uyruk="England", Yas=22, Mevki="Attacker", TakimAdi="Arsenal", TakimLogo="https://media.api-sports.io/football/teams/42.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=27, Goller=15, Asistler=10, Reyting=7.80, SariKartlar=2, KirmiziKartlar=0, OynananDakika=2380, Sutlar=70, IsabetliSutlar=40, Paslar=1100, PasIsabeti=85 } },
            new Oyuncu { Id=5, Ad="Mohamed Salah", Foto="https://media.api-sports.io/football/players/306.png", Uyruk="Egypt", Yas=31, Mevki="Attacker", TakimAdi="Liverpool", TakimLogo="https://media.api-sports.io/football/teams/40.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=27, Goller=17, Asistler=12, Reyting=8.10, SariKartlar=0, KirmiziKartlar=0, OynananDakika=2350, Sutlar=85, IsabetliSutlar=50, Paslar=850, PasIsabeti=81 } },
            new Oyuncu { Id=6, Ad="Ollie Watkins", Foto="https://media.api-sports.io/football/players/181.png", Uyruk="England", Yas=28, Mevki="Attacker", TakimAdi="Aston Villa", TakimLogo="https://media.api-sports.io/football/teams/66.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=28, Goller=16, Asistler=11, Reyting=7.75, SariKartlar=3, KirmiziKartlar=0, OynananDakika=2430, Sutlar=75, IsabetliSutlar=38, Paslar=600, PasIsabeti=71 } },
            new Oyuncu { Id=7, Ad="Jarrod Bowen", Foto="https://media.api-sports.io/football/players/200.png", Uyruk="England", Yas=27, Mevki="Attacker", TakimAdi="West Ham", TakimLogo="https://media.api-sports.io/football/teams/55.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=26, Goller=14, Asistler=6, Reyting=7.30, SariKartlar=4, KirmiziKartlar=0, OynananDakika=2200, Sutlar=65, IsabetliSutlar=35, Paslar=700, PasIsabeti=76 } },
            new Oyuncu { Id=8, Ad="Son Heung-min", Foto="https://media.api-sports.io/football/players/521.png", Uyruk="South Korea", Yas=31, Mevki="Attacker", TakimAdi="Tottenham", TakimLogo="https://media.api-sports.io/football/teams/47.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=27, Goller=13, Asistler=7, Reyting=7.20, SariKartlar=1, KirmiziKartlar=0, OynananDakika=2310, Sutlar=68, IsabetliSutlar=35, Paslar=780, PasIsabeti=79 } },
            new Oyuncu { Id=9, Ad="Dominic Solanke", Foto="https://media.api-sports.io/football/players/18.png", Uyruk="England", Yas=26, Mevki="Attacker", TakimAdi="Bournemouth", TakimLogo="https://media.api-sports.io/football/teams/35.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=28, Goller=13, Asistler=4, Reyting=7.00, SariKartlar=3, KirmiziKartlar=0, OynananDakika=2460, Sutlar=60, IsabetliSutlar=30, Paslar=550, PasIsabeti=68 } },
            new Oyuncu { Id=10, Ad="Leandro Trossard", Foto="https://media.api-sports.io/football/players/747.png", Uyruk="Belgium", Yas=29, Mevki="Midfielder", TakimAdi="Arsenal", TakimLogo="https://media.api-sports.io/football/teams/42.png", Istatistikler=new OyuncuIstatistikleri { MacSayisi=25, Goller=10, Asistler=5, Reyting=7.10, SariKartlar=2, KirmiziKartlar=0, OynananDakika=1900, Sutlar=55, IsabetliSutlar=28, Paslar=870, PasIsabeti=84 } },
        };

        private List<Mac> GetMockLiveMatches() => new()
        {
            new Mac { Id=1, EvSahibi="Liverpool", Deplasman="Arsenal", EvSahibiLogo="https://media.api-sports.io/football/teams/40.png", DeplasmanLogo="https://media.api-sports.io/football/teams/42.png", EvSahibiSkor=2, DeplasmanSkor=1, Durum="1H", Dakika=38, LigAdi="Premier League", LigLogo="https://media.api-sports.io/football/leagues/39.png", Tarih=DateTime.Now },
            new Mac { Id=2, EvSahibi="Real Madrid", Deplasman="Barcelona", EvSahibiLogo="https://media.api-sports.io/football/teams/541.png", DeplasmanLogo="https://media.api-sports.io/football/teams/529.png", EvSahibiSkor=1, DeplasmanSkor=1, Durum="2H", Dakika=67, LigAdi="La Liga", LigLogo="https://media.api-sports.io/football/leagues/140.png", Tarih=DateTime.Now },
        };

        private List<Mac> GetMockTodayMatches() => new()
        {
            new Mac { Id=1, EvSahibi="Liverpool", Deplasman="Arsenal", EvSahibiLogo="https://media.api-sports.io/football/teams/40.png", DeplasmanLogo="https://media.api-sports.io/football/teams/42.png", EvSahibiSkor=2, DeplasmanSkor=1, Durum="1H", Dakika=38, LigAdi="Premier League", LigLogo="https://media.api-sports.io/football/leagues/39.png", Tarih=DateTime.Now.AddHours(-1) },
            new Mac { Id=2, EvSahibi="Real Madrid", Deplasman="Barcelona", EvSahibiLogo="https://media.api-sports.io/football/teams/541.png", DeplasmanLogo="https://media.api-sports.io/football/teams/529.png", EvSahibiSkor=1, DeplasmanSkor=1, Durum="2H", Dakika=67, LigAdi="La Liga", LigLogo="https://media.api-sports.io/football/leagues/140.png", Tarih=DateTime.Now.AddHours(-1) },
            new Mac { Id=3, EvSahibi="Bayern Munich", Deplasman="Dortmund", EvSahibiLogo="https://media.api-sports.io/football/teams/157.png", DeplasmanLogo="https://media.api-sports.io/football/teams/165.png", EvSahibiSkor=null, DeplasmanSkor=null, Durum="NS", LigAdi="Bundesliga", LigLogo="https://media.api-sports.io/football/leagues/78.png", Tarih=DateTime.Now.AddHours(2) },
            new Mac { Id=4, EvSahibi="PSG", Deplasman="Lyon", EvSahibiLogo="https://media.api-sports.io/football/teams/85.png", DeplasmanLogo="https://media.api-sports.io/football/teams/80.png", EvSahibiSkor=null, DeplasmanSkor=null, Durum="NS", LigAdi="Ligue 1", LigLogo="https://media.api-sports.io/football/leagues/61.png", Tarih=DateTime.Now.AddHours(3) },
            new Mac { Id=5, EvSahibi="Manchester City", Deplasman="Chelsea", EvSahibiLogo="https://media.api-sports.io/football/teams/50.png", DeplasmanLogo="https://media.api-sports.io/football/teams/49.png", EvSahibiSkor=3, DeplasmanSkor=0, Durum="FT", LigAdi="Premier League", LigLogo="https://media.api-sports.io/football/leagues/39.png", Tarih=DateTime.Now.AddHours(-3) },
        };

        private List<Takim> GetMockTeams() => new()
        {
            new Takim { Id=33, Ad="Manchester City", Logo="https://media.api-sports.io/football/teams/50.png", Ulke="England", KurulusYili=1880, Stadyum="Etihad Stadium", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=21, Beraberlik=4, Maglubiyet=3, AtilanGol=62, YenilenGol=28 } },
            new Takim { Id=40, Ad="Liverpool", Logo="https://media.api-sports.io/football/teams/40.png", Ulke="England", KurulusYili=1892, Stadyum="Anfield", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=20, Beraberlik=5, Maglubiyet=3, AtilanGol=70, YenilenGol=30 } },
            new Takim { Id=42, Ad="Arsenal", Logo="https://media.api-sports.io/football/teams/42.png", Ulke="England", KurulusYili=1886, Stadyum="Emirates Stadium", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=18, Beraberlik=5, Maglubiyet=5, AtilanGol=65, YenilenGol=32 } },
            new Takim { Id=66, Ad="Aston Villa", Logo="https://media.api-sports.io/football/teams/66.png", Ulke="England", KurulusYili=1874, Stadyum="Villa Park", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=17, Beraberlik=4, Maglubiyet=7, AtilanGol=68, YenilenGol=52 } },
            new Takim { Id=49, Ad="Chelsea", Logo="https://media.api-sports.io/football/teams/49.png", Ulke="England", KurulusYili=1905, Stadyum="Stamford Bridge", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=14, Beraberlik=5, Maglubiyet=9, AtilanGol=60, YenilenGol=46 } },
            new Takim { Id=34, Ad="Newcastle", Logo="https://media.api-sports.io/football/teams/34.png", Ulke="England", KurulusYili=1892, Stadyum="St. James' Park", Istatistikler=new TakimIstatistikleri { Oynanan=28, Galibiyet=12, Beraberlik=7, Maglubiyet=9, AtilanGol=56, YenilenGol=42 } },
        };
    }
}

namespace Sportify.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Country { get; set; } = "";
        public int Founded { get; set; }
        public string Stadium { get; set; } = "";
        public TeamStats? Stats { get; set; }
    }

    public class TeamStats
    {
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDiff => GoalsFor - GoalsAgainst;
        public int Points => Won * 3 + Drawn;
        public double WinRate => Played > 0 ? Math.Round((double)Won / Played * 100, 1) : 0;
    }

    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Photo { get; set; } = "";
        public string Nationality { get; set; } = "";
        public int Age { get; set; }
        public string Position { get; set; } = "";
        public string TeamName { get; set; } = "";
        public string TeamLogo { get; set; } = "";
        public PlayerStats? Stats { get; set; }
    }

    public class PlayerStats
    {
        public int Appearances { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public double Rating { get; set; }
        public int MinutesPlayed { get; set; }
        public int Shots { get; set; }
        public int ShotsOnTarget { get; set; }
        public int Passes { get; set; }
        public double PassAccuracy { get; set; }
    }

    public class Standing
    {
        public int Rank { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string TeamLogo { get; set; } = "";
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDiff { get; set; }
        public int Points { get; set; }
        public string Form { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Country { get; set; } = "";
        public string Flag { get; set; } = "";
        public int Season { get; set; }
        public List<Standing> Standings { get; set; } = new();
    }

    public class Match
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public string HomeTeamLogo { get; set; } = "";
        public string AwayTeamLogo { get; set; } = "";
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public string Status { get; set; } = "";
        public string LeagueName { get; set; } = "";
        public string LeagueLogo { get; set; } = "";
        public DateTime Date { get; set; }
        public int? Minute { get; set; }
    }

    public class DashboardViewModel
    {
        public List<Match> LiveMatches { get; set; } = new();
        public List<Match> TodayMatches { get; set; } = new();
        public List<Standing> TopStandings { get; set; } = new();
        public List<Player> TopScorers { get; set; } = new();
        public string LeagueName { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}

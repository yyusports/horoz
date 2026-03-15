namespace Sportify.Models
{
    public class Takim
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Ulke { get; set; } = "";
        public int KurulusYili { get; set; }
        public string Stadyum { get; set; } = "";
        public TakimIstatistikleri? Istatistikler { get; set; }
    }

    public class TakimIstatistikleri
    {
        public int Oynanan { get; set; }
        public int Galibiyet { get; set; }
        public int Beraberlik { get; set; }
        public int Maglubiyet { get; set; }
        public int AtilanGol { get; set; }
        public int YenilenGol { get; set; }
        public int GolFarki => AtilanGol - YenilenGol;
        public int Puan => Galibiyet * 3 + Beraberlik;
        public double GalibiyetOrani => Oynanan > 0 ? Math.Round((double)Galibiyet / Oynanan * 100, 1) : 0;
    }

    public class Oyuncu
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Foto { get; set; } = "";
        public string Uyruk { get; set; } = "";
        public int Yas { get; set; }
        public string Mevki { get; set; } = "";
        public string TakimAdi { get; set; } = "";
        public string TakimLogo { get; set; } = "";
        public OyuncuIstatistikleri? Istatistikler { get; set; }
    }

    public class OyuncuIstatistikleri
    {
        public int MacSayisi { get; set; }
        public int Goller { get; set; }
        public int Asistler { get; set; }
        public int SariKartlar { get; set; }
        public int KirmiziKartlar { get; set; }
        public double Reyting { get; set; }
        public int OynananDakika { get; set; }
        public int Sutlar { get; set; }
        public int IsabetliSutlar { get; set; }
        public int Paslar { get; set; }
        public double PasIsabeti { get; set; }
    }

    public class PuanDurumu
    {
        public int Sira { get; set; }
        public int TakimId { get; set; }
        public string TakimAdi { get; set; } = "";
        public string TakimLogo { get; set; } = "";
        public int Oynanan { get; set; }
        public int Galibiyet { get; set; }
        public int Beraberlik { get; set; }
        public int Maglubiyet { get; set; }
        public int AtilanGol { get; set; }
        public int YenilenGol { get; set; }
        public int GolFarki { get; set; }
        public int Puan { get; set; }
        public string Form { get; set; } = "";
        public string Aciklama { get; set; } = "";
    }

    public class Lig
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Ulke { get; set; } = "";
        public string Bayrak { get; set; } = "";
        public int Sezon { get; set; }
        public List<PuanDurumu> PuanDurumlari { get; set; } = new();
    }

    public class Mac
    {
        public int Id { get; set; }
        public string EvSahibi { get; set; } = "";
        public string Deplasman { get; set; } = "";
        public string EvSahibiLogo { get; set; } = "";
        public string DeplasmanLogo { get; set; } = "";
        public int? EvSahibiSkor { get; set; }
        public int? DeplasmanSkor { get; set; }
        public string Durum { get; set; } = "";
        public string LigAdi { get; set; } = "";
        public string LigLogo { get; set; } = "";
        public DateTime Tarih { get; set; }
        public int? Dakika { get; set; }
    }

    public class DashboardViewModel
    {
        public List<Mac> CanliMaclar { get; set; } = new();
        public List<Mac> BugunMaclar { get; set; } = new();
        public List<PuanDurumu> UstSiraPuanDurumu { get; set; } = new();
        public List<Oyuncu> GolKralligi { get; set; } = new();
        public string LigAdi { get; set; } = "";
        public DateTime SonGuncelleme { get; set; } = DateTime.Now;
    }

    public class TeamsViewModel
    {
        public Dictionary<string, List<Takim>> LeaguedTeams { get; set; } = new();
        public HashSet<int> FavoriteTeamIds { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sportify.Models
{
    public class KullaniciTakim
    {
        public int KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; } = null!;

        public int TakimId { get; set; }
        public Takim Takim { get; set; } = null!;

        public DateTime EklenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}

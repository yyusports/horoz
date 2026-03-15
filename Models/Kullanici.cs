using System.ComponentModel.DataAnnotations;

namespace Sportify.Models
{
    public class Kullanici
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SifreHash { get; set; } = string.Empty;

        [Required]
        public string SifreSalt { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        
        public string? ProfilResmiYolu { get; set; }

        // E-posta doğrulama
        public string? EmailOnayKodu { get; set; }
        public bool IsEmailOnayli { get; set; } = false;
    }
}

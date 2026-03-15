using System.ComponentModel.DataAnnotations;

namespace Sportify.Models
{
    public class EpostaOnay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string OnayKodu { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
        
        public bool IsOnaylandi { get; set; } = false;
    }
}

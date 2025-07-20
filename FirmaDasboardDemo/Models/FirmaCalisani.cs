using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirmaDasboardDemo.Models
{
    public class FirmaCalisani
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string AdSoyad { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telefon { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sifre { get; set; }  // ✅ Şifre alanı eklendi

        // Foreign Key
        public int FirmaId { get; set; }
        public bool AktifMi { get; set; }
        public DateTime LisansSuresiBitis { get; set; }
        // Navigation
        [ForeignKey("FirmaId")]
        public Firma Firma { get; set; }
    }
}

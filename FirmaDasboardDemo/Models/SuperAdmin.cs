using System.ComponentModel.DataAnnotations;

namespace FirmaDasboardDemo.Models
{
    public class SuperAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Ad { get; set; }

        [Required]
        [MaxLength(50)]
        public string Soyad { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string Sifre { get; set; }

        // İstersen ek olarak login denetimi için:
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public bool AktifMi { get; set; } = true;
    }
}

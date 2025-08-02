using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirmaDasboardDemo.Models
{
    public class Bayi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ad { get; set; }

        [MaxLength(200)]
        public string Adres { get; set; }

        [MaxLength(50)]
        public string Il { get; set; }

        [MaxLength(50)]
        public string Ilce { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telefon { get; set; }
        public int RolId { get; set; }
        [ForeignKey("RolId")]
        public Rol Rol { get; set; }
        public bool KvkkOnaylandiMi { get; set; } = false;
        public bool EtkOnaylandiMi { get; set; } = false;

        [Required]
        [MaxLength(100)]
        public string Sifre { get; set; }  // ✅ Şifre alanı eklendi
        public bool AktifMi { get; set; }
        public DateTime LisansSuresiBitis { get; set; }
        // Bir bayinin birden fazla firmaya erişimi olabilir
        public ICollection<BayiFirma> BayiFirmalari { get; set; }
    }
}

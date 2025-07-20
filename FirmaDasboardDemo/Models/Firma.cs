using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FirmaDasboardDemo.Models;

namespace FirmaDasboardDemo.Models
{
    public class Firma
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
        // ➤ Yeni Alanlar
        public int MaxCalisanSayisi { get; set; }  // maksimum çalışan sayısı
        public int MaxBayiSayisi { get; set; }     // maksimum bayi sayısı
        public DateTime LisansBitisTarihi { get; set; } // lisans süres
        public bool AktifMi { get; set; }
        // Firma ile ilgili kullanıcılar (çalışanlar)
        public ICollection<FirmaCalisani> Calisanlar { get; set; }

        // Firmanın ilişkili olduğu bayiler (çoktan çoğa)
        public ICollection<BayiFirma> BayiFirmalari { get; set; }
    }
}

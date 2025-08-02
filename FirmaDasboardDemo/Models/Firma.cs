using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        // ➤ Lisans ve yönetimsel alanlar
        public int MaxCalisanSayisi { get; set; }
        public int MaxBayiSayisi { get; set; }
        public DateTime LisansBitisTarihi { get; set; }
        public bool AktifMi { get; set; }

        // ➤ SEO ve firma kimliği
        [MaxLength(100)]
        [Required]
        public string SeoUrl { get; set; }

        // 🖼️ Logo ve sosyal medya
        [MaxLength(250)]
        public string LogoUrl { get; set; }

        [MaxLength(250)]
        public string InstagramUrl { get; set; }

        [MaxLength(250)]
        public string FacebookUrl { get; set; }

        [MaxLength(250)]
        public string TwitterUrl { get; set; }

        [MaxLength(250)]
        public string WebSitesi { get; set; }

        // ➤ Navigation properties
        public ICollection<FirmaCalisani> Calisanlar { get; set; }
        public ICollection<BayiFirma> BayiFirmalari { get; set; }
    }
}

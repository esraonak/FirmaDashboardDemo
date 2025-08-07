using System;
using System.ComponentModel.DataAnnotations;

namespace FirmaDasboardDemo.Models
{
    public class MesajSatiri
    {
        public int Id { get; set; }

        // Foreign key
        public int BayiMesajId { get; set; }

        public int BayiId { get; set; }
        public int FirmaId { get; set; }
        public int? UrunId { get; set; }

        [Required]
        public string Icerik { get; set; }

        public bool GonderenFirmaMi { get; set; } // true: Firma, false: Bayi
        public DateTime Tarih { get; set; } = DateTime.Now;
        public bool OkunduMu { get; set; } = false;

        // 🔁 Navigation
        public BayiMesaj BayiMesaj { get; set; }
        public Bayi Bayi { get; set; }
        public Firma Firma { get; set; }
        public Urun Urun { get; set; }
    }
}

using FirmaDasboardDemo.Enums;
using System;
using System.Collections.Generic;

namespace FirmaDasboardDemo.Models
{
    public class BayiMesaj
    {
        public int Id { get; set; }

        public int BayiId { get; set; }
        public int FirmaId { get; set; }
        public int? UrunId { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;

        // Hesaplama varsa
        public string GirilenHucrelerJson { get; set; }
        public string GorunenHucrelerJson { get; set; }
        public string SatisFiyatlariJson { get; set; }

        public bool AktifMi { get; set; } = true;
        public bool FirmaGoruntulediMi { get; set; } = false;
        public bool BayiGoruntulediMi { get; set; } = true;

        public BayiMesajTuru MesajTuru { get; set; }

        // 🔁 Navigation
        public Bayi Bayi { get; set; }
        public Firma Firma { get; set; }
        public Urun? Urun { get; set; }

        public ICollection<MesajSatiri> Mesajlar { get; set; } = new List<MesajSatiri>();
    }
}

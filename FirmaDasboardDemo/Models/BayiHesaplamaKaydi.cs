namespace FirmaDasboardDemo.Models
{
    public class BayiHesaplamaKaydi
    {
        public int Id { get; set; }

        public int BayiId { get; set; }
        public int FirmaId { get; set; }
        public int UrunId { get; set; }

        public string GirilenHucrelerJson { get; set; } = "";
        public string GorunenHucrelerJson { get; set; } = "";
        public string SatisFiyatlariJson { get; set; } = "";

        public DateTime Tarih { get; set; } = DateTime.Now;
    }

}

namespace FirmaDasboardDemo.Models
{
    public class HataKaydi
    {
        public int Id { get; set; }
        public string? KullaniciRol { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? FirmaSeo { get; set; }
        public string? Aciklama { get; set; }
        public string? Url { get; set; } // ✅ Hangi URL'deyken oluştu
        public DateTime Tarih { get; set; }
    }

}

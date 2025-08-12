namespace FirmaDasboardDemo.Models
{
    public class SuperAdminHataKaydi
    {
        public int Id { get; set; }
        public string? KullaniciRol { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? FirmaSeo { get; set; }
        public string? Url { get; set; }
        public DateTime Tarih { get; set; }

        public string? HataMesaji { get; set; }   // ex.Message
        public string? StackTrace { get; set; }   // ex.ToString() (mesaj + stack)
    }
}

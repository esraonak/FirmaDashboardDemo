namespace FirmaDasboardDemo.Dtos
{
    public class FirmaUpdateDto
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Adres { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public int MaxCalisanSayisi { get; set; }
        public int MaxBayiSayisi { get; set; }
        public DateTime LisansBitisTarihi { get; set; }
        public bool AktifMi { get; set; }
        public string SeoUrl { get; set; }
        public string InstagramUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string WebSitesi { get; set; }
        public string LogoUrl { get; set; }
    }

}

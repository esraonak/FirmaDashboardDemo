namespace FirmaDasboardDemo.Dtos
{
    public class TabloKayitDto
    {
        public int TabloId { get; set; }           // Düzenleme için gerekli
        public int UrunId { get; set; }            // İlk kayıt için gerekli
        public string TabloAdi { get; set; }
        public string Aciklama { get; set; }
        public List<HucreDto> Hucreler { get; set; }
    }
}

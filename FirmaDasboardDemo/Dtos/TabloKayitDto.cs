namespace FirmaDasboardDemo.Dtos
{
    public class TabloKayitDto
    {
        public int UrunId { get; set; }
        public string TabloAdi { get; set; }
        public string Aciklama { get; set; }

        public List<HucreDto> Hucreler { get; set; }
    }
}

namespace FirmaDasboardDemo.Dtos
{
    public class BayiHesaplamaDto
    {
        public int UrunId { get; set; }
        public List<HucreDegeri> GirilenHucreler { get; set; }
        public List<HucreDegeri> GorunenHucreler { get; set; }
        public List<HucreDegeri> SatisFiyatlari { get; set; }
    }

    public class HucreDegeri
    {
        public string HucreAdi { get; set; }
        public string Deger { get; set; }
    }
}

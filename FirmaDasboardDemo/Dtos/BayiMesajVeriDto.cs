namespace FirmaDasboardDemo.Dtos
{
    public class BayiMesajVeriDto
    {
        public string UrunAdi { get; set; }
        public List<HucreDegeri> GirilenHucreler { get; set; }
        public List<HucreDegeri> GorunenHucreler { get; set; }
        public List<HucreDegeri> SatisFiyatlari { get; set; }
    }

}

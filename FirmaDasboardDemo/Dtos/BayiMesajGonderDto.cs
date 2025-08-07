using FirmaDasboardDemo.Enums; // ✅ Enum burada

namespace FirmaDasboardDemo.Dtos
{
    public class BayiMesajGonderDto
    {
        public int UrunId { get; set; }
        public string Mesaj { get; set; }

        public List<HucreDegeri> GirilenHucreler { get; set; }
        public List<HucreDegeri> GorunenHucreler { get; set; }
        public List<HucreDegeri> SatisFiyatlari { get; set; }

        public BayiMesajTuru MesajTuru { get; set; } // ✅ Enum türü
    }
}

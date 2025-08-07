using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Models;
using Newtonsoft.Json;


public class HesaplamaController : Controller
{
    private readonly ApplicationDbContext _context;

    public HesaplamaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("{firmaSeoUrl}/BayiHesaplamaKaydet")]
    public IActionResult BayiHesaplamaKaydet(
    string firmaSeoUrl,
    [FromForm] int UrunId,
    [FromForm] string GirilenHucreler,
    [FromForm] string GorunenHucreler,
    [FromForm] string SatisFiyatlari)
    {
        int? bayiId = HttpContext.Session.GetInt32("BayiId");
        int? firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (bayiId == null || firmaId == null)
            return Unauthorized();

        var girilen = JsonConvert.DeserializeObject<List<HucreDegeri>>(GirilenHucreler ?? "[]");
        var gorunen = JsonConvert.DeserializeObject<List<HucreDegeri>>(GorunenHucreler ?? "[]");
        var satislar = JsonConvert.DeserializeObject<List<HucreDegeri>>(SatisFiyatlari ?? "[]");

        var yeniKayit = new BayiHesaplamaKaydi
        {
            BayiId = bayiId.Value,
            FirmaId = firmaId.Value,
            UrunId = UrunId,
            GirilenHucrelerJson = JsonConvert.SerializeObject(girilen),
            GorunenHucrelerJson = JsonConvert.SerializeObject(gorunen),
            SatisFiyatlariJson = JsonConvert.SerializeObject(satislar),
            Tarih = DateTime.Now
        };

        // 🔴 Mevcut kayıtlar çekilir (en eskiye göre sıralanır)
        var mevcutKayitlar = _context.BayiHesaplamaKayitlari
            .Where(k => k.BayiId == bayiId.Value && k.UrunId == UrunId)
            .OrderByDescending(k => k.Tarih)
            .ToList();

        // 🧹 Eğer 10'dan fazlaysa, en eski olanlar silinir
        if (mevcutKayitlar.Count >= 10)
        {
            var silinecekler = mevcutKayitlar.Skip(9).ToList(); // 10’dan sonrası
            _context.BayiHesaplamaKayitlari.RemoveRange(silinecekler);
        }

        // 💾 Yeni kayıt eklenir
        _context.BayiHesaplamaKayitlari.Add(yeniKayit);
        _context.SaveChanges();

        return Ok();
    }


}

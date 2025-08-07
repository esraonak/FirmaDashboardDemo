using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
[Route("Error")]
public class ErrorController : Controller
{
    private readonly ApplicationDbContext _context;

    public ErrorController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    public IActionResult Index()
    {
        var rol = HttpContext.Session.GetString("UserRole") ?? "Bilinmiyor";
        var ad = HttpContext.Session.GetString("UserAd") ?? "Anonim";
        var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl") ?? "belirsiz";
        var sonUrl = HttpContext.Session.GetString("SonURL") ?? "Bilinmiyor";

        ViewBag.KullaniciRol = rol;
        ViewBag.SonUrl = sonUrl;

        return View();
    }


    [HttpPost("Bildir")]
    public IActionResult Bildir(string aciklama)

    {
        var rol = HttpContext.Session.GetString("UserRole") ?? "Bilinmiyor";
        var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl") ?? "belirsiz";

        var hata = new HataKaydi
        {
            KullaniciRol = rol,
            KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "Anonim",
            FirmaSeo = firmaSeo,
            Url = HttpContext.Session.GetString("SonURL") ?? "",
            Tarih = DateTime.Now,
            Aciklama = aciklama
        };

        _context.HataKayitlari.Add(hata);
        _context.SaveChanges();

        // 🎯 Role göre login URL’sini ViewBag’e aktar
        string yonlenUrl = rol switch
        {
            "Calisan" => $"/{firmaSeo}/Admin/Login",
            "Bayi" => $"/{firmaSeo}/Bayi/Login",
            _ => "/SuperAdmin/Login"
        };

        ViewBag.YonlendirmeUrl = yonlenUrl;
        return View("BildirimTesekkur"); // ➕ Teşekkür ekranı
    }


}

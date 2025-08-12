using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;   // 👈 IExceptionHandlerPathFeature
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

    // GET /Error
    [HttpGet("")]
    public IActionResult Index()
    {
        // Oturum bilgileri (sen zaten kullanıyorsun)
        var rol = HttpContext.Session.GetString("UserRole") ?? "Bilinmiyor";
        var ad = HttpContext.Session.GetString("UserAd") ?? "Anonim";
        var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl") ?? "belirsiz";
        var sonUrl = HttpContext.Session.GetString("SonURL") ?? "Bilinmiyor";

        // 🌶 Sunucu tarafı hatayı yakala (UseExceptionHandler bizi buraya yönlendirince)
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (feature?.Error != null)
        {
            try
            {
                var ex = feature.Error;

                _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
                {
                    KullaniciRol = rol,
                    KullaniciAdi = ad,
                    FirmaSeo = firmaSeo,
                    Url = feature.Path ?? sonUrl,
                    Tarih = DateTime.Now,
                    HataMesaji = ex.Message,
                    StackTrace = ex.ToString() // mesaj + stack birlikte
                });
                _context.SaveChanges();
            }
            catch
            {
                // DB'ye yazamazsak sayfayı düşürmeyelim
            }
        }

        ViewBag.KullaniciRol = rol;
        ViewBag.SonUrl = sonUrl;
        return View();
    }

    // POST /Error/Bildir  (mevcut kullanıcı bildirim akışın)
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

        string yonlenUrl = rol switch
        {
            "Calisan" => $"/{firmaSeo}/Admin/Login",
            "Bayi" => $"/{firmaSeo}/Bayi/Login",
            _ => "/SuperAdmin/Login"
        };

        ViewBag.YonlendirmeUrl = yonlenUrl;
        return View("BildirimTesekkur");
    }

    // (Opsiyonel) Frontend'ten JS ile log atmak için
    // POST /Error/LogClient  body: { message, detail }
    [HttpPost("LogClient")]
    public IActionResult LogClient([FromForm] string? message, [FromForm] string? detail)
    {
        try
        {
            var rol = HttpContext.Session.GetString("UserRole") ?? "Bilinmiyor";
            var ad = HttpContext.Session.GetString("UserAd") ?? "Anonim";
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl") ?? "belirsiz";
            var sonUrl = HttpContext.Session.GetString("SonURL") ?? HttpContext.Request.Path;

            _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
            {
                KullaniciRol = rol,
                KullaniciAdi = ad,
                FirmaSeo = firmaSeo,
                Url = sonUrl,
                Tarih = DateTime.Now,
                HataMesaji = message,
                StackTrace = detail
            });
            _context.SaveChanges();
        }
        catch { /* sessizce geç */ }

        return Ok(new { status = "ok" });
    }
}

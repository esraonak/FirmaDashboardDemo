using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Controllers;

namespace FirmaDashboardDemo.Controllers
{

    [Route("{firmaSeoUrl}/Admin")]
    public class CalisanController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public CalisanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult Login(string firmaSeoUrl)
        {
            if (string.IsNullOrWhiteSpace(firmaSeoUrl))
                return Content("Eksik bağlantı.");

            var firma = _context.Firmalar
                .AsEnumerable()
                .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));

            if (firma == null || !firma.AktifMi)
                return Content("Geçersiz firma bağlantısı.");

            // Session’a yaz
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);

            // ❗️ViewBag ile View’a gönder
            ViewBag.FirmaSeoUrl = firmaSeoUrl;

            return View();
        }


        [HttpPost("Login")]
        public IActionResult Login(string firmaSeoUrl, string Username, string Password)
        {
            var firma = _context.Firmalar
    .AsEnumerable()
    .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));


            if (firma == null || !firma.AktifMi)
                return Content("Firma bağlantısı geçersiz.");

            var calisan = _context.FirmaCalisanlari
                .FirstOrDefault(x => x.Email == Username &&
                                     x.Sifre == Password &&
                                     x.AktifMi &&
                                     x.FirmaId == firma.Id); // 🔒 Firma ile eşleştirildi mi?

            if (calisan == null)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
                return View();
            }

            // Session kayıtları
            HttpContext.Session.SetString("UserRole", "Calisan");
            HttpContext.Session.SetInt32("UserId", calisan.Id);
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);

            return Redirect("/" + firmaSeoUrl + "/Admin/Dashboard");
        }


        [HttpGet("Dashboard")]
        public IActionResult Dashboard(string firmaSeoUrl)
        {
            var calisanId = HttpContext.Session.GetInt32("UserId");
            var firmaSessionSeo = HttpContext.Session.GetString("FirmaSeoUrl");

            if (calisanId == null || string.IsNullOrEmpty(firmaSessionSeo) || !firmaSeoUrl.Equals(firmaSessionSeo, StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
            }

            return Redirect("/" + firmaSeoUrl + "/Admin/Bayi/BayiList");
        }


        [HttpGet("Logout")]
        public IActionResult Logout(string firmaSeoUrl)
        {
            HttpContext.Session.Clear();
            return Redirect("/" + firmaSeoUrl + "/Admin/Login");
        }

        // ✅ Tüm çalışanları getir (JSON)
        [HttpGet("Calisan/Calisanlar")]
        public IActionResult Calisanlar(string firmaSeoUrl)
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");

            var firma = _context.Firmalar.FirstOrDefault(x => x.Id == firmaId);
            if (firma != null)
                ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} ADMIN PANELİ";

            return View();
        }


        [HttpGet("Calisan/GetCalisanlar")]
        public IActionResult GetCalisanlar(string firmaSeoUrl)
        {
            var sessionSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            if (string.IsNullOrEmpty(sessionSeo) || !sessionSeo.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase))
                return Unauthorized();

            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var calisanlar = _context.FirmaCalisanlari
                .Where(c => c.FirmaId == firmaId)
                .Select(c => new
                {
                    c.Id,
                    c.AdSoyad,
                    c.Email,
                    c.Telefon,
                    c.AktifMi
                })
                .ToList();

            return Json(calisanlar);
        }


        // ✅ Yeni çalışan ekle
        [HttpPost]
        public IActionResult AddCalisan(FirmaCalisani model)
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return Unauthorized();
            var calisanrol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
            if (calisanrol == null)
                return BadRequest(new { status = "role_not_found" });
            model.FirmaId = firmaId.Value;
            model.Sifre = "1234"; // default şifre
            model.AktifMi = true;
            model.RolId = calisanrol.Id;
            _context.FirmaCalisanlari.Add(model);
            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        // ✅ Güncelleme
        [HttpPost]
        public IActionResult UpdateCalisan([FromBody] FirmaCalisani model)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == model.Id);
            if (calisan == null)
                return NotFound();

            calisan.AdSoyad = model.AdSoyad;
            calisan.Email = model.Email;
            calisan.Telefon = model.Telefon;
            calisan.AktifMi = model.AktifMi;

            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        // ✅ Silme
        [HttpPost]
        public IActionResult DeleteCalisan(int id)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == id);
            if (calisan == null)
                return NotFound();

            _context.FirmaCalisanlari.Remove(calisan);
            _context.SaveChanges();

            return Json(new { status = "deleted" });
        }

        // ✅ ID ile çalışan getir
        [HttpGet]
        public IActionResult GetCalisanById(int id)
        {
            var calisan = _context.FirmaCalisanlari
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.AdSoyad,
                    x.Email,
                    x.Telefon,
                    x.AktifMi
                })
                .FirstOrDefault();

            if (calisan == null)
                return NotFound();

            return Json(calisan);
        }
    }
}

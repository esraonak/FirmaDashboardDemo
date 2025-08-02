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

        public CalisanController(ApplicationDbContext context) : base(context)
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
            {
                TempData["LoginError"] = "Lisans süreniz dolmuştur. Lütfen yönetici ile iletişime geçin.";
                return View();
            }

            // ❗️ViewBag ile View’a gönder
            ViewBag.FirmaSeoUrl = firma.SeoUrl;
            ViewBag.FirmaAdi = firma.Ad;
            ViewBag.LogoUrl = string.IsNullOrEmpty(firma.LogoUrl) ? "/img/default-logo.png" : firma.LogoUrl;

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

            // ➕ KVKK & ETK onay kontrolü
            if (!calisan.KvkkOnaylandiMi || !calisan.EtkOnaylandiMi)
            {
                return RedirectToAction("OnayFormu", "Calisan", new { firmaSeoUrl = firma.SeoUrl });
            }

            // 🟢 Her şey yolundaysa dashboard'a yönlendir
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
        [HttpGet("KullaniciAyar")]
        public IActionResult KullaniciAyar()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return RedirectToAction("Login", "Calisan");

            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId);
            if (firma == null) return NotFound();

            ViewBag.Instagram = firma.InstagramUrl;
            ViewBag.Twitter = firma.TwitterUrl;
            ViewBag.Facebook = firma.FacebookUrl;
            ViewBag.Web = firma.WebSitesi;
            ViewBag.LogoUrl = firma.LogoUrl;

            // ❗️Route desteği için bu gerekli
            ViewBag.SirketSeoUrl = firma.SeoUrl;

            return View();
        }


        [HttpPost("SifreGuncelle")]
        public IActionResult SifreGuncelle(string MevcutSifre, string YeniSifre, string YeniSifreTekrar)
        {
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (firmaId == null || userId == null)
                return RedirectToAction("Login");

            var calisan = _context.FirmaCalisanlari.FirstOrDefault(c => c.Id == userId);
            if (calisan == null)
                return NotFound();

            if (calisan.Sifre != MevcutSifre)
            {
                TempData["Error"] = "Mevcut şifre yanlış.";
                
                return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                TempData["Error"] = "Yeni şifreler eşleşmiyor.";
                return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
            }

            calisan.Sifre = YeniSifre;
            _context.SaveChanges();

            TempData["Success"] = "Şifreniz başarıyla güncellendi.";

            // 🔄 Doğru SEO URL yönlendirmesi

            return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
        }


        [HttpPost("SosyalMedyaGuncelle")]
        public IActionResult SosyalMedyaGuncelle(string InstagramUrl, string TwitterUrl, string FacebookUrl, string WebSitesi)
        {
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return RedirectToAction("Login", "Calisan");

            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId);
            if (firma == null) return NotFound();

            firma.InstagramUrl = InstagramUrl;
            firma.TwitterUrl = TwitterUrl;
            firma.FacebookUrl = FacebookUrl;
            firma.WebSitesi = WebSitesi;

            _context.SaveChanges();

            TempData["Success"] = "Sosyal medya bağlantıları güncellendi.";
            return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
        }
        [HttpPost("LogoGuncelle")]
        public async Task<IActionResult> LogoGuncelle(IFormFile LogoFile)
        {
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return RedirectToAction("Login", "Calisan");

            if (LogoFile == null || LogoFile.Length == 0)
            {
                TempData["Error"] = "Lütfen bir logo dosyası seçin.";
                return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
            }

            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId);
            if (firma == null) return NotFound();

            var uzanti = Path.GetExtension(LogoFile.FileName);
            var fileName = $"firma_{firma.Id}_logo{uzanti}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await LogoFile.CopyToAsync(stream);
            }

            firma.LogoUrl = "/uploads/" + fileName;
            _context.SaveChanges();

            TempData["Success"] = "Logo başarıyla güncellendi.";
            return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
        }


        [HttpPost("AddCalisan")]
        public IActionResult AddCalisan(FirmaCalisani model)
        {
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return Unauthorized();

            bool emailExists = _context.FirmaCalisanlari.Any(c => c.Email == model.Email)
                || _context.Firmalar.Any(f => f.Email == model.Email)
                || _context.Bayiler.Any(b => b.Email == model.Email);

            if (emailExists)
                return Json(new { status = "email_exists" });

            var calisanRol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
            if (calisanRol == null)
                return BadRequest(new { status = "role_not_found" });

            model.FirmaId = firmaId.Value;
            model.Sifre = "1234";
            model.AktifMi = true;
            model.RolId = calisanRol.Id;

            _context.FirmaCalisanlari.Add(model);
            _context.SaveChanges();

            return Redirect("/" + firmaSeo + "/Admin/Calisan/Calisanlar");
        }


        [HttpPost("Calisan/UpdateCalisan")]
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
        [HttpPost("Calisan/DeleteCalisan")]
        public IActionResult DeleteCalisan([FromForm] int id)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == id);
            if (calisan == null)
                return NotFound();

            _context.FirmaCalisanlari.Remove(calisan);
            _context.SaveChanges();

            return Json(new { status = "deleted" });
        }



        // ✅ ID ile çalışan getir
        [HttpGet("Calisan/GetCalisanById/{id}")]
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

        [HttpGet("OnayFormu")]
        public IActionResult OnayFormu(string firmaSeoUrl)
        {
            ViewBag.FirmaSeoUrl = firmaSeoUrl;
            return View();
        }

        [HttpPost("OnayFormu")]
        [ValidateAntiForgeryToken]
        public IActionResult OnayFormu(string firmaSeoUrl, bool KvkkOnaylandiMi, bool EtkOnaylandiMi)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", new { firmaSeoUrl });

            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == userId);
            if (calisan == null) return RedirectToAction("Login", new { firmaSeoUrl });

            calisan.KvkkOnaylandiMi = KvkkOnaylandiMi;
            calisan.EtkOnaylandiMi = EtkOnaylandiMi;
            _context.SaveChanges();

            return Redirect("/" + firmaSeoUrl + "/Admin/Dashboard");
        }


    }
}

using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace FirmaDasboardDemo.Controllers
{
    [Route("SuperAdmin")]
    public class SuperAdminController : BaseSuperAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SuperAdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Rol") == "SuperAdmin")
                return RedirectToAction("Dashboard");
            if (TempData["Hata"] != null)
            {
                ViewBag.Hata = TempData["Hata"];
            }
            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login(string email, string sifre)
        {
            var admin = _context.SuperAdminler.FirstOrDefault(x => x.Email == email && x.Sifre == sifre && x.AktifMi);
            if (admin == null)
            {
                ViewBag.Hata = "E-posta veya şifre hatalı.";
                return View();
            }

            HttpContext.Session.SetInt32("SuperAdminId", admin.Id);
            HttpContext.Session.SetString("Rol", "SuperAdmin");
            HttpContext.Session.SetString("AdSoyad", $"{admin.Ad} {admin.Soyad}");

            return RedirectToAction("Dashboard");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Rol") != "SuperAdmin")
                return Unauthorized();
            if (TempData["Hata"] != null)
            {
                ViewBag.Hata = TempData["Hata"];
            }
            return View();
        }

        [HttpGet("GetFirmalar")]
        public IActionResult GetFirmalar()
        {
            var firmalar = _context.Firmalar.Select(f => new
            {
                f.Id,
                f.Ad,
                f.Email,
                f.Telefon,
                f.Il,
                f.Ilce,
                f.SeoUrl,
                f.AktifMi,
                f.LisansBitisTarihi
            }).ToList();

            return Json(firmalar);
        }

        [HttpPost("AddFirma")]
        public async Task<IActionResult> AddFirma(IFormCollection form)
        {
           

            // 🔒 Email veya Firma adı zaten varsa uyarı ver
            
            string ad = form["Ad"];
            string email = form["Email"];

            // ✅ Mevcut firma adı kontrolü
            if (_context.Firmalar.Any(f => f.Ad.ToLower() == ad.ToLower()))
            {
                TempData["Hata"] = "Bu firma adı zaten mevcut!";
                return RedirectToAction("Dashboard");
            }

            // ✅ Mevcut email kontrolü
            if (_context.Firmalar.Any(f => f.Email.ToLower() == email.ToLower()))
            {
                TempData["Hata"] = "Bu email adresi zaten kullanılıyor!";
                return RedirectToAction("Dashboard");
            }

            var firma = new Firma
            {
                Ad = ad,
                Email = email,
                Telefon = form["Telefon"],
                SeoUrl = form["SeoUrl"],
                LisansBitisTarihi = string.IsNullOrEmpty(form["LisansBitisTarihi"]) ? DateTime.Now : DateTime.Parse(form["LisansBitisTarihi"]),
                Adres = form["Adres"],
                Il = form["Il"],
                Ilce = form["Ilce"],
                InstagramUrl = form["InstagramUrl"],
                FacebookUrl = form["FacebookUrl"],
                TwitterUrl = form["TwitterUrl"],
                WebSitesi = form["WebSitesi"],
                MaxCalisanSayisi = int.TryParse(form["MaxCalisanSayisi"], out int mcs) ? mcs : 0,
                MaxBayiSayisi = int.TryParse(form["MaxBayiSayisi"], out int mbs) ? mbs : 0,
                AktifMi = form["AktifMi"] == "on"
            };

            var logoFile = form.Files["LogoFile"];
            if (logoFile != null && logoFile.Length > 0)
            {
                var logoFolder = Path.Combine(_env.WebRootPath, "firma-logos", firma.SeoUrl);
                if (!Directory.Exists(logoFolder))
                    Directory.CreateDirectory(logoFolder);

                var fileName = "logo.png";
                var fullPath = Path.Combine(logoFolder, fileName);

                using var stream = logoFile.OpenReadStream();
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(stream);

                int width = 300;
                int height = (int)(image.Height * (width / (double)image.Width));
                image.Mutate(x => x.Resize(width, height));
                await image.SaveAsPngAsync(fullPath);

                firma.LogoUrl = $"/firma-logos/{firma.SeoUrl}/{fileName}";
            }

            _context.Firmalar.Add(firma);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }



        [HttpPost("UpdateFirma")]
        public async Task<IActionResult> UpdateFirma(IFormCollection form)
        {
            int id = int.Parse(form["Id"]);
            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == id);
            if (firma == null) return NotFound();

            firma.Ad = form["Ad"];
            firma.Adres = form["Adres"];
            firma.Il = form["Il"];
            firma.Ilce = form["Ilce"];
            firma.Email = form["Email"];
            firma.Telefon = form["Telefon"];
            firma.MaxCalisanSayisi = int.TryParse(form["MaxCalisanSayisi"], out int mcs) ? mcs : 0;
            firma.MaxBayiSayisi = int.TryParse(form["MaxBayiSayisi"], out int mbs) ? mbs : 0;
            firma.LisansBitisTarihi = DateTime.Parse(form["LisansBitisTarihi"]);
            firma.AktifMi = form["AktifMi"] == "true" || form["AktifMi"] == "on";
            firma.SeoUrl = form["SeoUrl"];
            firma.InstagramUrl = form["InstagramUrl"];
            firma.FacebookUrl = form["FacebookUrl"];
            firma.TwitterUrl = form["TwitterUrl"];
            firma.WebSitesi = form["WebSitesi"];

            var logoFile = form.Files["LogoFile"];
            if (logoFile != null && logoFile.Length > 0)
            {
                // 📁 klasör: wwwroot/firma-logos/{SeoUrl}
                var logoFolder = Path.Combine(_env.WebRootPath, "firma-logos", firma.SeoUrl);
                if (!Directory.Exists(logoFolder))
                    Directory.CreateDirectory(logoFolder);

                var fileName = "logo.png";
                var fullPath = Path.Combine(logoFolder, fileName);

                using (var stream = logoFile.OpenReadStream())
                using (var image = await SixLabors.ImageSharp.Image.LoadAsync(stream))
                {
                    int width = 300;
                    int height = (int)(image.Height * (width / (double)image.Width));
                    image.Mutate(x => x.Resize(width, height));
                    await image.SaveAsPngAsync(fullPath);
                }

                firma.LogoUrl = $"/firma-logos/{firma.SeoUrl}/{fileName}";
            }

            // 🔁 Firma pasif yapılırsa bayi ve çalışanlar da pasif yapılmalı
            var bayiler = _context.Bayiler
                .Where(b => b.BayiFirmalari.Any(bf => bf.FirmaId == firma.Id))
                .ToList();
            foreach (var bayi in bayiler)
                bayi.AktifMi = firma.AktifMi;

            var calisanlar = _context.FirmaCalisanlari.Where(c => c.FirmaId == firma.Id).ToList();
            foreach (var calisan in calisanlar)
                calisan.AktifMi = firma.AktifMi;

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }



        [HttpPost("DeleteFirma")]
        public IActionResult DeleteFirma(int id)
        {
            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == id);
            if (firma == null)
                return Json(new { status = "not_found" });

            firma.AktifMi = false;

            // ✅ Firma çalışanlarını pasif yap
            var calisanlar = _context.FirmaCalisanlari.Where(c => c.FirmaId == id).ToList();
            foreach (var c in calisanlar)
            {
                c.AktifMi = false;
            }

            // ✅ Bu firmaya bağlı bayi ilişkilerini al → Include ile Bayi'leri dahil et
            var bayiList = _context.BayiFirmalari
                .Include(bf => bf.Bayi)
                .Where(bf => bf.FirmaId == id)
                .Select(bf => bf.Bayi)
                .Distinct()
                .ToList();

            foreach (var bayi in bayiList)
            {
                bayi.AktifMi = false;
                // İsteğe bağlı: bayi.LisansSuresiBitis = DateTime.Now;
            }

            _context.SaveChanges();

            return Json(new { status = "deleted" });
        }

        [HttpGet("GetFirmaById")]
        public IActionResult GetFirmaById(int id)
        {
            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == id);
            if (firma == null)
                return NotFound();

            return Json(firma);
        }


        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        [HttpPost("AddCalisan")]
        public IActionResult AddCalisan(IFormCollection form)
        {
            if (!int.TryParse(form["FirmaId"], out int firmaId))
                return Json(new { status = "invalid_firma" });

            string email = form["Email"];
            bool emailExists = _context.FirmaCalisanlari.Any(c => c.Email == email)
                || _context.Firmalar.Any(f => f.Email == email)
                || _context.Bayiler.Any(b => b.Email == email);

            if (emailExists)
                return Json(new { status = "email_exists" });

            var rol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
            if (rol == null)
                return Json(new { status = "role_not_found" });

            var calisan = new FirmaCalisani
            {
                AdSoyad = form["AdSoyad"],
                Email = email,
                Telefon = form["Telefon"],
                Sifre = form["Sifre"],
                AktifMi = form["AktifMi"] == "on",
                RolId = rol.Id,
                FirmaId = firmaId,
                LisansSuresiBitis = DateTime.Now.AddYears(1)
            };

            _context.FirmaCalisanlari.Add(calisan);
            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        [HttpGet("Calisanlar")]
        public IActionResult Calisanlar()
        {
            return View();
        }
        [HttpGet("GetCalisanlar")]
        public IActionResult GetCalisanlar()
        {
            var calisanlar = _context.FirmaCalisanlari
                .Include(c => c.Firma)
                .Select(c => new {
                    id = c.Id,
                    adSoyad = c.AdSoyad,
                    email = c.Email,
                    telefon = c.Telefon,
                    firmaAd = c.Firma.Ad,
                    aktifMi = c.AktifMi
                })
                .ToList();

            return Json(calisanlar);
        }
        [HttpGet("CalisanGetir")]
        public IActionResult CalisanGetir(int id)
        {
            var calisan = _context.FirmaCalisanlari
                .Where(c => c.Id == id)
                .Select(c => new {
                    c.Id,
                    adSoyad = c.AdSoyad,
                    c.Email,
                    c.Telefon,
                    c.AktifMi
                })
                .FirstOrDefault();

            if (calisan == null) return NotFound();
            return Json(calisan);
        }

        [HttpPost("CalisanGuncelle")]
        public IActionResult CalisanGuncelle(int id, string adSoyad, string email, string telefon, bool aktifMi)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(c => c.Id == id);
            if (calisan == null) return NotFound();

            calisan.AdSoyad = adSoyad;
            calisan.Email = email;
            calisan.Telefon = telefon;
            calisan.AktifMi = aktifMi;

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost("CalisanPasifYap")]
        public IActionResult CalisanPasifYap(int id)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(c => c.Id == id);
            if (calisan == null) return NotFound();

            calisan.AktifMi = false;
            _context.SaveChanges();
            return Json(new { success = true });
        }




        [HttpGet("Bayiler")]
        public IActionResult Bayiler()
        {
            return View();
        }
        [HttpGet("GetBayiler")]
        public IActionResult GetBayiler()
        {
            var bayiler = _context.Bayiler
      .Include(b => b.BayiFirmalari)
          .ThenInclude(bf => bf.Firma)
      .Select(b => new
      {
          id = b.Id,
          adSoyad = b.Ad,
          email = b.Email,
          telefon = b.Telefon,
          // Firma adlarını virgülle birleştir
          firmaAd = string.Join(", ", b.BayiFirmalari.Select(bf => bf.Firma.Ad)),
          lisansBitis = b.LisansSuresiBitis.ToString("dd.MM.yyyy"),
          aktifMi = b.AktifMi 
      }).ToList();

            return Json(bayiler);
        }
        [HttpGet("BayiGetir")]
        public IActionResult BayiGetir(int id)
        {
            var bayi = _context.Bayiler
                .Select(b => new {
                    b.Id,
                    adSoyad = b.Ad,
                    b.Email,
                    b.Telefon,
                    lisansBitis = b.LisansSuresiBitis,
                    aktifMi = b.AktifMi
                })
                .FirstOrDefault(b => b.Id == id);

            if (bayi == null)
                return NotFound();

            return Json(bayi);
        }
        [HttpPost("BayiGuncelle")]
        public IActionResult BayiGuncelle(int id, string adSoyad, string email, string telefon, DateTime lisansBitis, bool aktifMi)
        {
            var bayi = _context.Bayiler.FirstOrDefault(b => b.Id == id);
            if (bayi == null)
                return NotFound();

            bayi.Ad = adSoyad;
            bayi.Email = email;
            bayi.Telefon = telefon;
            bayi.LisansSuresiBitis = lisansBitis;
            bayi.AktifMi = aktifMi;

            _context.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost("BayiSil")]
        public IActionResult BayiSil(int id)
        {
            var bayi = _context.Bayiler.FirstOrDefault(b => b.Id == id);
            if (bayi == null)
                return Json(new { success = false, message = "Bayi bulunamadı." });

            bayi.AktifMi = false;
            _context.SaveChanges();

            return Json(new { success = true });
        }





        [HttpGet("KullaniciAyarlar")]
        public IActionResult KullaniciAyarlar()
        {
            return View();
        }
        [HttpPost("GuncelleSifre")]
        public IActionResult GuncelleSifre(string EskiSifre, string YeniSifre, string YeniSifreTekrar)
        {
            int? superAdminId = HttpContext.Session.GetInt32("SuperAdminId");
            if (superAdminId == null)
                return Json(new { status = "error", message = "Giriş yapılmamış." });

            var admin = _context.SuperAdminler.FirstOrDefault(x => x.Id == superAdminId);
            if (admin == null || admin.Sifre != EskiSifre) // 🔐 hash yoksa bu kadar basit
                return Json(new { status = "error", message = "Eski şifre yanlış." });

            if (YeniSifre != YeniSifreTekrar)
                return Json(new { status = "error", message = "Yeni şifreler uyuşmuyor." });

            admin.Sifre = YeniSifre;
            _context.SaveChanges();

            return Json(new { status = "success", message = "Şifre başarıyla güncellendi." });
        }


        [HttpGet("Lisanslar")]
        public IActionResult Lisanslar()
        {
            return View();
        }
        [HttpGet("GetLisanslar")]
        public IActionResult GetLisanslar()
        {
            var lisanslar = _context.Firmalar
                .Select(f => new
                {
                    id = f.Id,
                    ad = f.Ad,
                    seoUrl = f.SeoUrl,
                    lisansBitisTarihi = f.LisansBitisTarihi,
                    aktifMi = f.AktifMi
                }).ToList();

            return Json(lisanslar);
        }
        [HttpPost("GuncelleLisans")]
        public IActionResult GuncelleLisans(int id, DateTime lisansBitis, bool aktifMi)
        {
            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == id);
            if (firma == null)
                return NotFound();

            firma.LisansBitisTarihi = lisansBitis;
            firma.AktifMi = aktifMi;
            _context.SaveChanges();

            return Ok();
        }

    }
}

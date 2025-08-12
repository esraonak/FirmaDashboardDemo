using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Helpers;
using Microsoft.Extensions.Configuration;
namespace FirmaDasboardDemo.Controllers
{
    [Route("SuperAdmin")]
    public class SuperAdminController : BaseSuperAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public SuperAdminController(ApplicationDbContext context, IWebHostEnvironment env, IConfiguration configuration) // IConfiguration parametresi ekle
        {
            _context = context;
            _env = env;
            _configuration = configuration; // ATAMA YAP
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
            // 🔧 Bunlar hata sistemi için gerekli
            HttpContext.Session.SetString("UserRole", "SuperAdmin");
            HttpContext.Session.SetString("UserAd", $"{admin.Ad} {admin.Soyad}");
            HttpContext.Session.SetString("FirmaSeoUrl", "superadmin"); // 🔄 isteğe bağlı bir sabit değer

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
            try
            {
                var ad = (form["Ad"].ToString() ?? "").Trim();
                var email = (form["Email"].ToString() ?? "").Trim();
                var seo = (form["SeoUrl"].ToString() ?? "").Trim();

                if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(seo))
                {
                    TempData["Hata"] = "Firma Adı ve Seo URL zorunludur.";
                    return RedirectToAction("Dashboard");
                }

                if (_context.Firmalar.Any(f => f.Ad.ToLower() == ad.ToLower()))
                {
                    TempData["Hata"] = "Bu firma adı zaten mevcut!";
                    return RedirectToAction("Dashboard");
                }

                if (!string.IsNullOrEmpty(email) &&
                    _context.Firmalar.Any(f => (f.Email ?? "").ToLower() == email.ToLower()))
                {
                    TempData["Hata"] = "Bu email adresi zaten kullanılıyor!";
                    return RedirectToAction("Dashboard");
                }

                // varsayılan logo (NOT NULL kolon güvenliği)
                const string defaultLogo = "/assets/img/default-logo.png";

                var firma = new Firma
                {
                    Ad = ad,
                    Email = email,
                    Telefon = form["Telefon"],
                    SeoUrl = seo,
                    LisansBitisTarihi = string.IsNullOrEmpty(form["LisansBitisTarihi"])
                                            ? DateTime.Now
                                            : DateTime.Parse(form["LisansBitisTarihi"]),
                    Adres = form["Adres"],
                    Il = form["Il"],
                    Ilce = form["Ilce"],
                    InstagramUrl = form["InstagramUrl"],
                    FacebookUrl = form["FacebookUrl"],
                    TwitterUrl = form["TwitterUrl"],
                    WebSitesi = form["WebSitesi"],
                    MaxCalisanSayisi = int.TryParse(form["MaxCalisanSayisi"], out var mcs) ? mcs : 0,
                    MaxBayiSayisi = int.TryParse(form["MaxBayiSayisi"], out var mbs) ? mbs : 0,
                    AktifMi = form["AktifMi"] == "on",
                    LogoUrl = defaultLogo
                };

                var logoFile = form.Files["LogoFile"];
                if (logoFile != null && logoFile.Length > 0)
                {
                    // WebRoot fallback + güvenli klasör oluşturma
                    var webRoot = _env.WebRootPath;
                    if (string.IsNullOrWhiteSpace(webRoot))
                        webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    var logoFolder = Path.Combine(webRoot, "firma-logos", firma.SeoUrl);

                    try
                    {
                        Directory.CreateDirectory(logoFolder);          // yoksa oluştur
                        var fullPath = Path.Combine(logoFolder, "logo.png");

                        using var stream = logoFile.OpenReadStream();
                        using var image = await SixLabors.ImageSharp.Image.LoadAsync(stream);

                        var width = 300;
                        var height = (int)(image.Height * (width / (double)image.Width));
                        image.Mutate(x => x.Resize(width, height));
                        await image.SaveAsPngAsync(fullPath);

                        firma.LogoUrl = $"/firma-logos/{firma.SeoUrl}/logo.png";
                    }
                    catch (UnauthorizedAccessException uaex)
                    {
                        // Yetki hatası: default logoda bırak, kaydı yine de al
                        _context.HataKayitlari.Add(new HataKaydi
                        {
                            KullaniciRol = "SuperAdmin",
                            KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "SuperAdmin",
                            FirmaSeo = "superadmin",
                            Url = "/SuperAdmin/AddFirma",
                            Tarih = DateTime.Now,
                            Aciklama = "Logo klasörü yazma yetkisi yok: " + uaex.Message
                        });

                        await _context.SaveChangesAsync();

                        firma.LogoUrl = defaultLogo;
                    }
                }

                _context.Firmalar.Add(firma);
                await _context.SaveChangesAsync();

                TempData["Mesaj"] = "Firma başarıyla eklendi.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _context.HataKayitlari.Add(new HataKaydi
                {
                    KullaniciRol = "SuperAdmin",
                    KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "SuperAdmin",
                    FirmaSeo = "superadmin",
                    Url = "/SuperAdmin/AddFirma",
                    Tarih = DateTime.Now,
                    Aciklama = ex.Message
                });
                await _context.SaveChangesAsync();

                TempData["Hata"] = "Firma eklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }


        [HttpPost("UpdateFirma")]
        public IActionResult UpdateFirma([FromBody] FirmaUpdateDto dto)
        {
            var firma = _context.Firmalar.FirstOrDefault(f => f.Id == dto.Id);
            if (firma == null)
                return NotFound();

            // ✅ Lisans tarihi kontrolü
            if (dto.AktifMi && dto.LisansBitisTarihi <= DateTime.Today)
            {
                return Json(new
                {
                    status = "error",
                    message = "Firma aktif yapılamaz. Lisans bitiş tarihi bugünden ileri olmalıdır."
                });
            }

            firma.Ad = dto.Ad;
            firma.Adres = dto.Adres;
            firma.Il = dto.Il;
            firma.Ilce = dto.Ilce;
            firma.Email = dto.Email;
            firma.Telefon = dto.Telefon;
            firma.MaxCalisanSayisi = dto.MaxCalisanSayisi;
            firma.MaxBayiSayisi = dto.MaxBayiSayisi;
            firma.LisansBitisTarihi = dto.LisansBitisTarihi;
            firma.AktifMi = dto.AktifMi;
            firma.SeoUrl = dto.SeoUrl;
            firma.InstagramUrl = dto.InstagramUrl;
            firma.FacebookUrl = dto.FacebookUrl;
            firma.TwitterUrl = dto.TwitterUrl;
            firma.WebSitesi = dto.WebSitesi;
            firma.LogoUrl = dto.LogoUrl;

            // bayi ve calisan aktiflik eşitleme
            var bayiler = _context.Bayiler
                .Where(b => b.BayiFirmalari.Any(bf => bf.FirmaId == firma.Id))
                .ToList();

            foreach (var bayi in bayiler)
                bayi.AktifMi = firma.AktifMi;

            var calisanlar = _context.FirmaCalisanlari
                .Where(c => c.FirmaId == firma.Id)
                .ToList();

            foreach (var calisan in calisanlar)
                calisan.AktifMi = firma.AktifMi;

            _context.SaveChanges();

            return Json(new { status = "success" });
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
            try
            {
                if (!int.TryParse(form["FirmaId"], out int firmaId))
                    return Json(new { status = "invalid_firma", message = "FirmaId geçersiz." });

                string adSoyad = (form["AdSoyad"].ToString() ?? "").Trim();
                string email = (form["Email"].ToString() ?? "").Trim();
                string telefon = (form["Telefon"].ToString() ?? "").Trim();
                bool aktifMi = form["AktifMi"] == "on";

                if (string.IsNullOrWhiteSpace(adSoyad) || string.IsNullOrWhiteSpace(email))
                    return Json(new { status = "validation_error", message = "Ad Soyad ve Email zorunludur." });

                bool emailExists =
                    _context.FirmaCalisanlari.Any(c => c.Email == email) ||
                    _context.Firmalar.Any(f => f.Email == email) ||
                    _context.Bayiler.Any(b => b.Email == email);

                if (emailExists)
                    return Json(new { status = "email_exists", message = "Bu e-posta adresi zaten kullanılıyor." });

                var rol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
                if (rol == null)
                    return Json(new { status = "role_not_found", message = "Çalışan rolü tanımlı değil." });

                var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId);
                if (firma == null)
                    return Json(new { status = "firma_not_found", message = "Firma bulunamadı." });

                if (aktifMi && (!firma.AktifMi || firma.LisansBitisTarihi <= DateTime.Today))
                    return Json(new { status = "firma_pasif", message = "Firma pasif veya lisansı bitmiş." });

                // 1) Rastgele şifre
                string randomPassword = SifreUretici.RastgeleSifreUret(10);

                // 2) Login URL
                string baseUrl = _configuration["Sistem:BaseUrl"] ?? "https://tentecrm.com.tr";
                string loginUrl = $"{baseUrl}/{firma.SeoUrl}/Admin/Login";

                // 3) Firma mailine bilgilendirme (best-effort)
                if (!string.IsNullOrWhiteSpace(firma.Email))
                    MailHelper.MailGonderDetay(firma.Email, "Yeni Çalışan Bilgisi",
                        $"{adSoyad} adlı çalışan eklendi.\nEmail: {email}\nTelefon: {telefon}");

                // 4) Çalışana mail – başarısızsa kaydetmeyelim
                string body =
                    $"Merhaba {adSoyad},\n\n" +
                    $"TenteCRM hesabınız oluşturuldu.\n\n" +
                    $"Giriş URL: {loginUrl}\n" +
                    $"Kullanıcı Adı: {email}\n" +
                    $"Şifreniz: {randomPassword}\n\n" +
                    $"Giriş yaptıktan sonra lütfen şifrenizi değiştirin.";

                var (ok, error) = MailHelper.MailGonderDetay(email, "TenteCRM Hesap Bilgileriniz", body);
                if (!ok)
                {
                    _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
                    {
                        KullaniciRol = HttpContext.Session.GetString("UserRole") ?? "SuperAdmin",
                        KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "SuperAdmin",
                        FirmaSeo = firma.SeoUrl,
                        Url = HttpContext.Request?.Path.Value,
                        Tarih = DateTime.Now,
                        HataMesaji = "SMTP hata: " + (error ?? "-"),
                        StackTrace = error
                    });
                    _context.SaveChanges();

                    return Json(new { status = "mail_error", message = error ?? "E-posta gönderilemedi." });
                }

                // 5) Kaydet (HASH)
                var calisan = new FirmaCalisani
                {
                    AdSoyad = adSoyad,
                    Email = email,
                    Telefon = telefon,
                    Sifre = HashHelper.Hash(randomPassword),
                    AktifMi = aktifMi,
                    RolId = rol.Id,
                    FirmaId = firmaId,
                    LisansSuresiBitis = DateTime.Now.AddYears(1)
                };

                _context.FirmaCalisanlari.Add(calisan);
                _context.SaveChanges();

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                try
                {
                    _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
                    {
                        KullaniciRol = HttpContext.Session.GetString("UserRole") ?? "SuperAdmin",
                        KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "SuperAdmin",
                        FirmaSeo = HttpContext.Session.GetString("FirmaSeoUrl") ?? "superadmin",
                        Url = HttpContext.Request?.Path.Value,
                        Tarih = DateTime.Now,
                        HataMesaji = ex.Message,
                        StackTrace = ex.ToString()
                    });
                    _context.SaveChanges();
                }
                catch { /* yut */ }

                return Json(new { status = "error", message = "Çalışan eklenirken bir hata oluştu." });
            }
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
            var calisan = _context.FirmaCalisanlari
                .Include(c => c.Firma)
                .FirstOrDefault(c => c.Id == id);

            if (calisan == null)
                return NotFound();

            // ✅ Firma aktif değilse ve çalışan aktif yapılmak isteniyorsa engelle
            if (aktifMi)
            {
                var firma = calisan.Firma;
                if (firma == null || !firma.AktifMi || firma.LisansBitisTarihi <= DateTime.Today)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Firma pasif durumda veya lisans süresi dolmuş. Çalışanı aktif hale getiremezsiniz."
                    });

                }
            }

            // ✅ Güncelleme işlemi
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
                .Include(b => b.BayiFirmalari)
                .ThenInclude(bf => bf.Firma)
                .FirstOrDefault(b => b.Id == id);

            if (bayi == null) return NotFound();

            return Json(new
            {
                id = bayi.Id,
                adSoyad = bayi.Ad,
                email = bayi.Email,
                telefon = bayi.Telefon,
                // 🟢 Önemli: Tarih formatını doğrudan yyyy-MM-dd olarak gönder
                lisansBitis = bayi.LisansSuresiBitis == DateTime.MinValue
                              ? null
                              : bayi.LisansSuresiBitis.ToString("yyyy-MM-dd"),
                aktifMi = bayi.AktifMi
            });
        }


        [HttpPost("BayiGuncelle")]
        public IActionResult BayiGuncelle(int id, string adSoyad, string email, string telefon, DateTime lisansBitis, bool aktifMi)
        {
            var bayi = _context.Bayiler
                .Include(b => b.BayiFirmalari)
                    .ThenInclude(bf => bf.Firma)
                .FirstOrDefault(b => b.Id == id);

            if (bayi == null)
                return NotFound();

            // ✅ Bağlı olduğu aktif bir firma var mı kontrolü
            if (aktifMi)
            {
                var aktifFirmaVarMi = bayi.BayiFirmalari.Any(bf =>
                    bf.Firma != null &&
                    bf.Firma.AktifMi &&
                    bf.Firma.LisansBitisTarihi > DateTime.Today);

                if (!aktifFirmaVarMi)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Bayi aktif hale getirilemez. Bağlı olduğu firmanın lisansı bitmiş veya pasif durumda."
                    });
                }
            }

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



        [Route("SuperAdmin/Hatalar")]
        public IActionResult Hatalar()
        {
            var kayitlar = _context.HataKayitlari
                .OrderByDescending(h => h.Tarih)
                .ToList();
            return View(kayitlar);
        }
        [HttpGet("TestHata")]
        public IActionResult TestHata()
        {
            throw new Exception("Test amaçlı bilinçli bir hata oluştu.");
        }
        [HttpGet("/SuperAdmin/HataKayitlari")]
        public IActionResult HataKayitlari()
        {
            return View(); // sadece görünüm sayfasını döner
        }

        [HttpGet("/SuperAdmin/api/hatalar")]
        public IActionResult HatalariGetir()
        {
            var kayitlar = _context.HataKayitlari
                .OrderByDescending(h => h.Tarih)
                .Select(h => new {
                    h.Id,
                    h.KullaniciAdi,
                    h.KullaniciRol,
                    h.FirmaSeo,
                    h.Url,
                    Tarih = h.Tarih.ToString("dd.MM.yyyy HH:mm"),
                    h.Aciklama
                })
                .ToList();

            return Json(new { data = kayitlar });
        }

        [HttpPost("/SuperAdmin/api/hata-sil")]
        public IActionResult HataSil(int id)
        {
            var kayit = _context.HataKayitlari.Find(id);
            if (kayit == null)
                return NotFound();

            _context.HataKayitlari.Remove(kayit);
            _context.SaveChanges();
            return Ok(new { status = "success" });
        }

       

    }
}

using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Controllers;
using System.Globalization;
using FirmaDasboardDemo.Helpers;

namespace FirmaDashboardDemo.Controllers
{

    [Route("{firmaSeoUrl}/Admin")]
    public class CalisanController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public CalisanController(
          ApplicationDbContext context,
          IConfiguration configuration
      ) : base(context)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpGet("Login")]
        public IActionResult Login(string firmaSeoUrl)
        {
            // 🚫 SEO URL boşsa bağlantı eksik
            if (string.IsNullOrWhiteSpace(firmaSeoUrl))
                return Content("Eksik bağlantı.");

            // 🔍 Firma doğrulaması yapılır (case-insensitive)
            var firma = _context.Firmalar
                .AsEnumerable()
                .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));

            // ❌ Firma bulunamazsa ya da pasifse, uyarı döner
            if (firma == null || !firma.AktifMi)
            {
                TempData["LoginError"] = "Lisans süreniz dolmuştur. Lütfen yönetici ile iletişime geçin.";
                return View();
            }

            // ✅ Giriş ekranına firma bilgilerini gönder
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

            var hashliSifre = HashHelper.Hash(Password);
            var calisan = _context.FirmaCalisanlari
                .FirstOrDefault(x => x.Email == Username &&
                                     x.Sifre == hashliSifre &&
                                     x.AktifMi &&
                                     x.FirmaId == firma.Id);

            if (calisan == null)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
                return View();
            }
            // Session kayıtları
            // Session kayıtları
            HttpContext.Session.SetString("UserRole", "Calisan");
            HttpContext.Session.SetInt32("CalisanId", calisan.Id);
            HttpContext.Session.SetInt32("UserId", calisan.Id);
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("UserAd", calisan.AdSoyad); // ➕ eksik olan eklendi


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
            var calisanId = HttpContext.Session.GetInt32("CalisanId");
            var firmaSessionSeo = HttpContext.Session.GetString("FirmaSeoUrl");

            if (calisanId == null || string.IsNullOrEmpty(firmaSessionSeo) || !firmaSeoUrl.Equals(firmaSessionSeo, StringComparison.OrdinalIgnoreCase))
            {
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
            }

            // 📌 Firma sosyal medya linklerini ViewBag'e ata
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == firmaSeoUrl);
            if (firma != null)
            {
                ViewBag.FirmaSosyalMedya = new
                {
                    Instagram = string.IsNullOrEmpty(firma.InstagramUrl) ? "#" : firma.InstagramUrl,
                    Twitter = string.IsNullOrEmpty(firma.TwitterUrl) ? "#" : firma.TwitterUrl,
                    Facebook = string.IsNullOrEmpty(firma.FacebookUrl) ? "#" : firma.FacebookUrl,
                    Website = string.IsNullOrEmpty(firma.WebSitesi) ? "#" : firma.WebSitesi
                };

                ViewBag.FirmaLogo = firma.LogoUrl; // varsa logo da set et
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });

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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            int? userId = HttpContext.Session.GetInt32("CalisanId");

            if (firmaId == null || userId == null)
                return RedirectToAction("Login");

            var calisan = _context.FirmaCalisanlari.FirstOrDefault(c => c.Id == userId);
            if (calisan == null)
                return NotFound();
            var mevcutHash = HashHelper.Hash(MevcutSifre);
            if (calisan.Sifre != mevcutHash)
            {
                TempData["Error"] = "Mevcut şifre yanlış.";
                
                return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                TempData["Error"] = "Yeni şifreler eşleşmiyor.";
                return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
            }

            calisan.Sifre = HashHelper.Hash(YeniSifre);

            _context.SaveChanges();

            TempData["Success"] = "Şifreniz başarıyla güncellendi.";

            // 🔄 Doğru SEO URL yönlendirmesi

            return Redirect("/" + firmaSeo + "/Admin/KullaniciAyar");
        }


        [HttpPost("SosyalMedyaGuncelle")]
        public IActionResult SosyalMedyaGuncelle(string InstagramUrl, string TwitterUrl, string FacebookUrl, string WebSitesi)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            try
            {
                // 0) Oturum ve firma kontrolü
                var calisanSessionId = HttpContext.Session.GetInt32("CalisanId");
                var firmaId = HttpContext.Session.GetInt32("FirmaId");
                var firmaSeo = HttpContext.Session.GetString("FirmaSeoUrl");

                if (calisanSessionId == null || firmaId == null)
                {
                    return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = firmaSeo });
                }

                var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId.Value);
                if (firma == null)
                    return BadRequest(new { status = "firma_not_found" });

                // 1) Limit ve tekillik kontrolleri
                int mevcutCalisan = _context.FirmaCalisanlari.Count(c => c.FirmaId == firma.Id);
                if (firma.MaxCalisanSayisi > 0 && mevcutCalisan >= firma.MaxCalisanSayisi)
                    return Json(new { status = "max_calisan_limit", message = "Maksimum çalışan limitine ulaşıldı." });

                string email = (model.Email ?? "").Trim();
                string adSoyad = (model.AdSoyad ?? "").Trim();
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(adSoyad))
                    return Json(new { status = "validation_error", message = "Ad Soyad ve E-posta zorunludur." });

                bool emailExists =
                    _context.FirmaCalisanlari.Any(c => c.Email == email && c.FirmaId == firma.Id) ||
                    _context.Bayiler.Any(b => b.Email == email && b.BayiFirmalari.Any(bf => bf.FirmaId == firma.Id)) ||
                    string.Equals(firma.Email ?? "", email, StringComparison.OrdinalIgnoreCase);

                if (emailExists)
                    return Json(new { status = "email_exists", message = "Bu e-posta zaten kullanılıyor." });

                var calisanRol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
                if (calisanRol == null)
                    return BadRequest(new { status = "role_not_found" });

                // Firma pasif / lisansı bitmişse aktif çalışan açmayı engelle
                if ((!firma.AktifMi || firma.LisansBitisTarihi <= DateTime.Today))
                    return Json(new { status = "firma_pasif", message = "Firma pasif veya lisansı bitmiş." });

                // 2) Rastgele şifre + login URL
                string rawPassword = SifreUretici.RastgeleSifreUret(10);
                string baseUrl = _configuration["Sistem:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = $"{Request.Scheme}://{Request.Host}"; // fallback

                string loginUrl = $"{baseUrl}/{firma.SeoUrl}/Admin/Login";

                // 3) Mail – başarısızsa kayıt yapma
                string konu = "TenteCRM Giriş Bilgileriniz";
                string icerik =
                    $"Merhaba {adSoyad},\n\n" +
                    $"TenteCRM hesabınız oluşturuldu.\n\n" +
                    $"Giriş URL: {loginUrl}\n" +
                    $"Kullanıcı Adı: {email}\n" +
                    $"Şifreniz: {rawPassword}\n\n" +
                    $"Giriş yaptıktan sonra lütfen şifrenizi değiştirin.";

                var (ok, error) = MailHelper.MailGonderDetay(email, konu, icerik);
                if (!ok)
                {
                    // İstersen logla
                     _context.HataKayitlari.Add(new HataKaydi {
                         KullaniciRol = "Calisan",
                         KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "-",
                         FirmaSeo     = firma.SeoUrl,
                        Url          = HttpContext.Request?.Path.Value,
                       Tarih        = DateTime.Now,
                         Aciklama     = "SMTP hata: " + (error ?? "-")
                    });
                     _context.SaveChanges();

                    return Json(new { status = "mail_error", message = error ?? "E-posta gönderilemedi." });
                }

                // 4) Mail başarılıysa DB’ye kaydet (hash’li şifre)
                model.FirmaId = firma.Id;
                model.RolId = calisanRol.Id;
                model.AktifMi = true;
                model.Sifre = HashHelper.Hash(rawPassword);
                model.LisansSuresiBitis = DateTime.Now.AddYears(1);

                _context.FirmaCalisanlari.Add(model);
                _context.SaveChanges();

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                // İstersen burada da loglayabilirsin
                // _context.HataKayitlari.Add(new HataKaydi { ... });
                // _context.SaveChanges();

                return Json(new { status = "unexpected_error", message = ex.Message });
            }
        }




        [HttpPost("Calisan/UpdateCalisan")]
        public IActionResult UpdateCalisan([FromBody] FirmaCalisani model)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == model.Id);
            if (calisan == null)
                return NotFound();

            var firmaId = calisan.FirmaId;

            // 📧 E-posta kontrolü (aynı firmada ve farklı kullanıcıya aitse)
            bool emailExists = _context.FirmaCalisanlari
                .Any(c => c.Id != model.Id && c.Email == model.Email && c.FirmaId == firmaId)
                || _context.Bayiler
                .Any(b => b.Email == model.Email && b.BayiFirmalari.Any(bf => bf.FirmaId == firmaId))
                || _context.Firmalar
                .Any(f => f.Id == firmaId && f.Email == model.Email);

            if (emailExists)
            {
                return Json(new { status = "email_exists" });
            }

            // 📝 Güncelleme işlemi
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (userId == null) return RedirectToAction("Login", new { firmaSeoUrl });
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == calisanId);
            
            if (calisan == null) return RedirectToAction("Login", new { firmaSeoUrl });

            calisan.KvkkOnaylandiMi = KvkkOnaylandiMi;
            calisan.EtkOnaylandiMi = EtkOnaylandiMi;
            _context.SaveChanges();

            return Redirect("/" + firmaSeoUrl + "/Admin/Dashboard");
        }


        [HttpGet("GetOkunmamisMesajSayisi")]
        public IActionResult GetOkunmamisBayiMesajlari([FromRoute(Name = "firmaSeoUrl")] string seo)
        {
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seo);
            if (firma == null) return NotFound();

            // 🔁 Firmanın cevaplamadığı (yani bayiden gelen) tüm mesajları çek
            var mesajlar = _context.MesajSatirlari
                .Include(m => m.BayiMesaj)
                .ThenInclude(bm => bm.Bayi)
                .Where(m => m.BayiMesaj.FirmaId == firma.Id && !m.GonderenFirmaMi && !m.OkunduMu)
                .OrderByDescending(m => m.Tarih)
                .Select(m => new
                {
                    Id = m.BayiMesaj.BayiId,
                    Ad = m.BayiMesaj.Bayi.Ad,
                    Tarih = m.Tarih
                })
                .ToList();

            return Json(mesajlar);
        }

        [HttpPost("Mesaj/Sil/{id}")]
        public IActionResult MesajSil(int id)
        {
            var mesaj = _context.BayiMesajlar.FirstOrDefault(m => m.Id == id);
            if (mesaj == null)
                return NotFound();

            _context.BayiMesajlar.Remove(mesaj);
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpPost("Mesaj/Kapat/{id}")]
        public IActionResult TicketKapat(int id)
        {
            var mesaj = _context.BayiMesajlar.FirstOrDefault(m => m.Id == id);
            if (mesaj == null)
                return NotFound();

            mesaj.AktifMi = false;
            _context.SaveChanges();

            return Ok(new { success = true });
        }


        

        [HttpGet("Bayi/Mesajlar")]
        public IActionResult Mesajlar(string firmaSeoUrl, int? bayiId)
        {
            var firma = _context.Firmalar
                .AsEnumerable()
                .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));

            if (firma == null)
            {
                TempData["Hata"] = "Firma bulunamadı.";
                return RedirectToAction("Dashboard", "Admin", new { firmaSeoUrl });
            }

            var mesajlar = _context.BayiMesajlar
                .Include(m => m.Bayi)
                .Include(m => m.Urun)
                .Include(m => m.Mesajlar)
                .Where(m => m.FirmaId == firma.Id)
                .ToList();

            // Bayi listesi ViewBag'e
            var bayiler = mesajlar
                .Select(m => m.Bayi)
                .Where(b => b != null)
                .Distinct()
                .OrderBy(b => b.Ad)
                .ToList();

            // Firma tarafından okunmamış mesajı olan bayiler
            var okunmamisBayiIdListesi = mesajlar
                .Where(m => m.Mesajlar.Any(x => !x.GonderenFirmaMi && !x.OkunduMu))
                .Select(m => m.BayiId)
                .Distinct()
                .ToList();

            ViewBag.Bayiler = bayiler;
            ViewBag.OkunmamisBayiIdListesi = okunmamisBayiIdListesi;
            ViewBag.AktifBayiId = bayiId;
            ViewBag.FirmaSeo = firmaSeoUrl;

            // Eğer filtre uygulanmışsa sadece ilgili bayiye ait mesajları göster
            var filtrelenmis = bayiId.HasValue
                ? mesajlar.Where(m => m.BayiId == bayiId.Value).OrderByDescending(m => m.Tarih).ToList()
                : mesajlar.OrderByDescending(m => m.Tarih).ToList();

            return View("~/Views/Calisan/Bayi/Mesajlar.cshtml", filtrelenmis);
        }


        [HttpGet("Bayi/MesajDetay/{id}")]
        public IActionResult MesajDetay(string firmaSeoUrl, int id)
        {
            // 🔍 Firma kontrolü
            var firma = _context.Firmalar
                 .AsEnumerable()
                 .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));
            if (firma == null)
            {
                TempData["Hata"] = "Firma bulunamadı.";
                return RedirectToAction("Dashboard", "Calisan", new { firmaSeoUrl });
            }

            // 🔒 Çalışan oturumu kontrolü
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
            {
                TempData["Hata"] = "Çalışan oturumu bulunamadı.";
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl });
            }

            // 📩 Mesaj getir (yalnızca firma üzerinden)
            var bayiMesaj = _context.BayiMesajlar
                .Include(m => m.Firma)
                .Include(m => m.Bayi)
                .Include(m => m.Urun)
                .Include(m => m.Mesajlar.OrderBy(m => m.Tarih))
                .FirstOrDefault(m => m.Id == id && m.FirmaId == firma.Id);

            if (bayiMesaj == null)
            {
                TempData["Hata"] = "Mesaj bulunamadı.";
                return RedirectToAction("Mesajlar", "Calisan", new { firmaSeoUrl });
            }

            // 🟢 Okunmamış bayi mesajlarını okundu işaretle
            foreach (var mesaj in bayiMesaj.Mesajlar.Where(m => !m.GonderenFirmaMi && !m.OkunduMu))
                mesaj.OkunduMu = true;

            // 📌 Ana mesaj kayıt durumu
            bayiMesaj.FirmaGoruntulediMi = true;
            _context.SaveChanges();

            // 🌐 ViewBag ile SEO URL View tarafına aktarılıyor
            ViewBag.FirmaSeo = firmaSeoUrl;

            // ✅ View'e yönlendir
            return View("~/Views/Calisan/Bayi/MesajDetay.cshtml", bayiMesaj);
        }



        [HttpPost("Bayi/MesajCevapla")]
        public IActionResult MesajCevapla(string firmaSeoUrl, int MesajId, string Cevap)
        {
            var bayiMesaj = _context.BayiMesajlar
                .Include(m => m.Bayi)
                .Include(m => m.Urun)
                .FirstOrDefault(m => m.Id == MesajId);

            if (bayiMesaj == null)
            {
                TempData["Hata"] = "Mesaj bulunamadı.";
                return RedirectToAction("Mesajlar", new { firmaSeoUrl });
            }

            var yeniSatir = new MesajSatiri
            {
                BayiId = bayiMesaj.BayiId,
                FirmaId = bayiMesaj.FirmaId,
                UrunId = bayiMesaj.UrunId,
                BayiMesajId = bayiMesaj.Id,
                Icerik = Cevap?.Trim(),
                GonderenFirmaMi = true,
                OkunduMu = false,
                Tarih = DateTime.Now
            };

            _context.MesajSatirlari.Add(yeniSatir);
            _context.SaveChanges();
            ViewBag.FirmaSeo = firmaSeoUrl;
            TempData["Basari"] = "Yanıt başarıyla gönderildi.";
            return RedirectToAction("MesajDetay", new { firmaSeoUrl, id = MesajId });
        }


    }
}

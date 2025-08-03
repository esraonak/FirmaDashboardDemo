using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Controllers;


namespace FirmaDashboardDemo.Controllers
{
    [Route("{firmaSeoUrl}/Bayi")]
    public class BayiSayfasiController : BaseBayiController
    {
        private readonly ApplicationDbContext _context;

        public BayiSayfasiController(ApplicationDbContext context) : base(context)
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
                return RedirectToAction("Login", new { firmaSeoUrl }); // 🔁 recursive redirect, doğru parametreyle
            }

            // ✅ Session’a firma bilgilerini ve kullanıcı rolünü yaz
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("FirmaLogo", firma.LogoUrl ?? ""); // 🔁 logo destekli
            HttpContext.Session.SetString("UserRole", "Bayi");
            HttpContext.Session.SetString("Instagram", firma.InstagramUrl ?? "");
            HttpContext.Session.SetString("Twitter", firma.TwitterUrl ?? "");
            HttpContext.Session.SetString("Facebook", firma.FacebookUrl ?? "");
            HttpContext.Session.SetString("WebSitesi", firma.WebSitesi ?? "");

            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} BAYİ PANELİ";

            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login(string Username, string Password)
        {
            var bayi = _context.Bayiler.FirstOrDefault(b => b.Email == Username && b.Sifre == Password && b.AktifMi);
            if (bayi == null)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre";
                return View();
            }

            // 🔐 FirmaSeoUrl session’dan alınmalı (önceki GET çağrısından)
            var firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl");
            if (string.IsNullOrEmpty(firmaSeoUrl))
                return Content("Firma bilgisi eksik.");

            // 🔍 SeoUrl ile firmayı bul
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == firmaSeoUrl && f.AktifMi);
            if (firma == null)
                return Content("Firma bulunamadı.");

            // 🔁 Bu bayi o firmayla eşleşiyor mu? (BayiFirma tablosundan kontrol)
            var eslesmeVarMi = _context.BayiFirmalari
                .Any(bf => bf.BayiId == bayi.Id && bf.FirmaId == firma.Id);
            if (!eslesmeVarMi)
                return Content("Bu firmayla ilişkilendirilmiş bayi hesabı bulunamadı.");

            // ✅ Session kayıtları
            HttpContext.Session.SetInt32("UserId", bayi.Id);
            HttpContext.Session.SetInt32("RolId", bayi.RolId);
            HttpContext.Session.SetString("UserAd", bayi.Ad);
            HttpContext.Session.SetString("UserRole", "Bayi");

            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaLogo", firma.LogoUrl ?? "");
            // ✅ Sosyal medya bilgilerini Session'a yaz
            HttpContext.Session.SetString("Instagram", firma.InstagramUrl ?? "");
            HttpContext.Session.SetString("Twitter", firma.TwitterUrl ?? "");
            HttpContext.Session.SetString("Facebook", firma.FacebookUrl ?? "");
            HttpContext.Session.SetString("WebSitesi", firma.WebSitesi ?? "");
            // ✅ KVKK & ETK kontrol
            if (!bayi.KvkkOnaylandiMi || !bayi.EtkOnaylandiMi)
            {
                return RedirectToAction("OnayFormu", "BayiSayfasi", new { firmaSeoUrl = firma.SeoUrl });
            }

            return Redirect("/" + firma.SeoUrl + "/Bayi/Dashboard");
        }



        [HttpGet("Dashboard")]
        public IActionResult Dashboard(string firmaSeoUrl)
        {
            var bayiId = HttpContext.Session.GetInt32("UserId");
            if (bayiId == null)
                return Redirect("/" + firmaSeoUrl + "/Bayi/Dashboard");

            ViewBag.PanelBaslik = $"{firmaSeoUrl.ToUpper()} BAYİ PANELİ";
            return View("BayiDashboard");
        }



        // ✅ Seçilen firmaya ait ürünleri getirir
        [HttpGet("BayiSayfasi/GetUrunler")]
        public IActionResult GetUrunler()
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId)
                .Where(u => _context.FormulTablosu.Any(ft => ft.UrunId == u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    ad = u.Ad
                })
                .ToList();

            return Json(urunler);
        }


        // ✅ Seçilen ürünün bayi tarafından görüntülenebilir formül tablosunu getirir
        [HttpGet("BayiSayfasi/GetTablo")]
        public IActionResult GetTablo(int urunId)
        {
            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.UrunId == urunId);

            if (tablo == null)
                return NotFound();

            // ❗️TÜM hücreleri alıyoruz artık
            var hucreListesi = tablo.Hucreler.ToList();

            // Satır/sütun sayısını tüm hücrelere göre ayarla
            int maxRow = hucreListesi
                .Select(h => int.Parse(System.Text.RegularExpressions.Regex.Match(h.HucreAdi, @"\d+").Value))
                .DefaultIfEmpty(0)
                .Max();

            int maxCol = hucreListesi
                .Select(h => h.HucreAdi[0] - 'A' + 1)
                .DefaultIfEmpty(0)
                .Max();

            var hucreMap = hucreListesi.ToDictionary(h => h.HucreAdi, h => h);

            // Grid oluştur
            var grid = new List<List<string>>();
            for (int r = 0; r < maxRow; r++)
            {
                var row = new List<string>();
                for (int c = 0; c < maxCol; c++)
                {
                    var hucreAdi = $"{(char)('A' + c)}{r + 1}";
                    if (hucreMap.TryGetValue(hucreAdi, out var hucre))
                        row.Add(hucre.Formul);
                    else
                        row.Add("");
                }
                grid.Add(row);
            }

            return Json(new
            {
                hucreler = hucreListesi.Select(h => new
                {
                    hucreAdi = h.HucreAdi,
                    gozuksunMu = h.GozuksunMu,
                    girdimiYapabilir = h.GirdimiYapabilir,
                    formulMu = h.IsFormul
                }),
                grid
            });
        }
        [HttpGet("KullaniciAyar")]
        public IActionResult KullaniciAyar()
        {

            return View();
        }

        [HttpPost("KullaniciAyar")]
        public IActionResult KullaniciAyarGuncelle(string EskiSifre, string YeniSifre, string YeniSifreTekrar)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            if (bayiId == null)
                return RedirectToAction("Login");

            var bayi = _context.Bayiler.Find(bayiId);
            if (bayi == null)
            {
                TempData["Error"] = "Bayi bulunamadı.";
                return RedirectToAction("KullaniciAyar");
            }

            if (bayi.Sifre != EskiSifre)
            {
                TempData["Error"] = "Mevcut şifre hatalı.";
                return RedirectToAction("KullaniciAyar");
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                TempData["Error"] = "Yeni şifreler uyuşmuyor.";
                return RedirectToAction("KullaniciAyar");
            }

            bayi.Sifre = YeniSifre;
            _context.SaveChanges();

            TempData["Success"] = "Şifre başarıyla güncellendi.";
            return RedirectToAction("KullaniciAyar");
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
            if (userId == null)
                return RedirectToAction("Login", "BayiSayfasi", new { firmaSeoUrl });

            var bayi = _context.Bayiler.FirstOrDefault(b => b.Id == userId);
            if (bayi == null)
                return RedirectToAction("Login", "BayiSayfasi", new { firmaSeoUrl });

            bayi.KvkkOnaylandiMi = KvkkOnaylandiMi;
            bayi.EtkOnaylandiMi = EtkOnaylandiMi;

            _context.SaveChanges();

            return Redirect("/" + firmaSeoUrl + "/Bayi/Dashboard");
        }






    }
}

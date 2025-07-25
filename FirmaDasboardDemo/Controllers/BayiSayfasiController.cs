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

        public BayiSayfasiController(ApplicationDbContext context)
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

            // ✅ Session’a firma bilgilerini ve kullanıcı rolünü yaz
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("UserRole", "Bayi"); // ❗️EKLENDİ

            // ✅ Layout için ViewBag başlık
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

            // ✅ Zorunlu Session kayıtları
            HttpContext.Session.SetInt32("UserId", bayi.Id);
            HttpContext.Session.SetInt32("RolId", bayi.RolId);
            HttpContext.Session.SetString("UserAd", bayi.Ad);
            HttpContext.Session.SetString("UserRole", "Bayi"); // ❗️EKLENDİ

            // ✅ Firma bilgisini session'dan al
            var firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl");
            if (string.IsNullOrEmpty(firmaSeoUrl))
            {
                return Content("Firma bilgisi eksik.");
            }

            // (İsteğe bağlı) Panel başlığı ayarlanabilir ama View’a dönülmediği için zorunlu değil
            ViewBag.PanelBaslik = $"{firmaSeoUrl.ToUpper()} BAYİ PANELİ";

            // ✅ Dashboard’a yönlendir
            return Redirect("/" + firmaSeoUrl + "/Bayi/Dashboard");
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
    






    }
}

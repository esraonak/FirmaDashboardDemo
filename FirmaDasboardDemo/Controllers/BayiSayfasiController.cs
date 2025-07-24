using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using FirmaDasboardDemo.Dtos;
using NCalc;

namespace FirmaDashboardDemo.Controllers
{
    public class BayiSayfasiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BayiSayfasiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); // -> Views/BayiSayfasi/Login.cshtml dosyasına yönlendirir
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            var bayi = _context.Bayiler.FirstOrDefault(b => b.Email == Username && b.Sifre == Password && b.AktifMi);
            if (bayi == null)
            {
                ViewBag.Error = "Geçersiz kullanıcı adı veya şifre";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", bayi.Id);
            HttpContext.Session.SetInt32("RolId", bayi.RolId);
            HttpContext.Session.SetString("UserAd", bayi.Ad);

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var bayiId = HttpContext.Session.GetInt32("UserId");
            if (bayiId == null)
                return RedirectToAction("Login", "Bayi");

            return View("BayiDashboard");
        }

        // ✅ Bayinin kayıtlı olduğu firmaları getirir
        [HttpGet("BayiSayfasi/GetFirmalarim")]
        public IActionResult GetFirmalarim()
        {
            var bayiId = HttpContext.Session.GetInt32("UserId");
            if (bayiId == null)
                return Unauthorized();

            var firmalar = _context.BayiFirmalari
                .Include(bf => bf.Firma)
                .Where(bf => bf.BayiId == bayiId)
                .Select(bf => new
                {
                    id = bf.Firma.Id,
                    ad = bf.Firma.Ad
                })
                .ToList();

            return Json(firmalar);
        }

        // ✅ Seçilen firmaya ait ürünleri getirir
        [HttpGet("BayiSayfasi/GetUrunler")]
        public IActionResult GetUrunler(int firmaId)
        {
            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId)
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

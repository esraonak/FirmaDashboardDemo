using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Models; // FormulTablosu ve Hucre modeli için gerekli

namespace FirmaDasboardDemo.Controllers
{
    public class TabloController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TabloController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("api/urun/ekle")]
        public IActionResult UrunEkle([FromBody] UrunEkleDto dto)
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();
            // 🔁 Aynı adda ürün varsa uyarı dön
            bool ayniAdVar = _context.Urun.Any(u => u.FirmaId == firmaId && u.Ad.ToLower() == dto.Ad.ToLower());
            if (ayniAdVar)
            {
                return Ok(new { status = "duplicate" });
            }

            var urun = new Urun
            {
                Ad = dto.Ad,
                Aciklama = dto.Aciklama,
                FirmaId = firmaId.Value
            };

            _context.Urun.Add(urun);
            _context.SaveChanges();

            return Ok(new { id = urun.Id, ad = urun.Ad });
        }

        [HttpGet]
        [Route("api/urun/liste")]
        public IActionResult UrunleriGetir()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId && !_context.FormulTablosu.Any(t => t.UrunId == u.Id))
                .Select(u => new { id = u.Id, ad = u.Ad })
                .ToList();

            return Ok(urunler);
        }
        [HttpGet]
        [Route("api/urun/tabloluurun")]
        public IActionResult TablosuOlanUrunleriGetir()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            // ❗️Sadece FormulTablosu kaydı olan ürünleri getir
            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId && _context.FormulTablosu.Any(t => t.UrunId == u.Id))
                .Select(u => new { id = u.Id, ad = u.Ad })
                .ToList();

            return Ok(urunler);
        }

        // GET: /Tablo/TabloOlustur
        [HttpGet]
        public IActionResult TabloOlustur()
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return RedirectToAction("Login", "Calisan");

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId)
                .Select(u => new { u.Id, u.Ad })
                .ToList();

            ViewBag.Urunler = urunler;

            // View: Views/Calisan/TabloOlustur.cshtml
            return View("~/Views/Calisan/TabloOlustur.cshtml");
        }

        // POST: /api/tablo/kaydet
        [HttpPost("api/tablo/kaydet")]
        public IActionResult Kaydet([FromBody] TabloKayitDto input)
        {
            var calisanId = HttpContext.Session.GetInt32("UserId");
            var firmaId = HttpContext.Session.GetInt32("FirmaId");

            if (calisanId == null || firmaId == null)
                return Unauthorized();

            // Aynı ürün için tablo var mı kontrol
            var mevcut = _context.FormulTablosu.Any(t => t.UrunId == input.UrunId);
            if (mevcut)
            {
                return Ok(new { status = "already_exists" });
            }

            var tablo = new FormulTablosu
            {
                Ad = input.TabloAdi,
                Aciklama = input.Aciklama,
                UrunId = input.UrunId,
                CalisanId = calisanId.Value,
                OlusturmaTarihi = DateTime.Now,
                Hucreler = input.Hucreler.Select(h => new Hucre
                {
                    HucreAdi = h.HucreAdi,
                    Formul = h.Formul ?? "",
                    IsFormul = h.IsFormul,
                    // Girebilir olan hücre otomatik olarak görünür olacak
                    GozuksunMu = h.GirdimiYapabilir ? true : h.GozuksunMu,
                    GirdimiYapabilir = h.GirdimiYapabilir
                }).ToList()
            };

            _context.FormulTablosu.Add(tablo);
            _context.SaveChanges();

            return Ok(new { status = "ok", tabloId = tablo.Id });
        }


        [HttpGet]
        public IActionResult TabloDuzenle()
        {
            return View("~/Views/Calisan/TabloDuzenle.cshtml"); // otomatik olarak Views/Tablo/TabloDuzenle.cshtml'yi bulur
        }
        [HttpGet]
        public IActionResult VeriGetir(int urunId)
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return Unauthorized();

            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.UrunId == urunId && t.Urun.FirmaId == firmaId);

            if (tablo == null)
                return NotFound(new { status = "not_found" });

            var hucreler = tablo.Hucreler.Select(h => new
            {
                h.HucreAdi,
                h.Formul,
                h.IsFormul,
                h.GozuksunMu,
                h.GirdimiYapabilir
            });

            return Json(new { tabloId = tablo.Id, hucreler });
        }

        [HttpPost]
        public IActionResult Guncelle([FromBody] TabloKayitDto dto)
        {
            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.Id == dto.TabloId);

            if (tablo == null)
                return NotFound(new { status = "not_found" });

            _context.Hucre.RemoveRange(tablo.Hucreler);

            tablo.Hucreler = dto.Hucreler.Select(h => new Hucre
            {
                HucreAdi = h.HucreAdi,
                Formul = h.Formul ?? "",
                IsFormul = h.IsFormul,
                GozuksunMu = h.GirdimiYapabilir ? true : h.GozuksunMu,
                GirdimiYapabilir = h.GirdimiYapabilir
            }).ToList();

            _context.SaveChanges();

            return Ok(new { status = "ok" });
        }


        [HttpGet]
        public IActionResult TabloSil()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return RedirectToAction("Login", "Calisan");

            var tablolar = _context.FormulTablosu
                .Include(t => t.Urun)
                .Where(t => t.Urun.FirmaId == firmaId)
                .Select(t => new {
                    Id = t.Id,
                    Ad = t.Ad,
                    UrunAd = t.Urun.Ad
                })
                .ToList();

            ViewBag.Tablolar = tablolar;

            return View("~/Views/Calisan/TabloSil.cshtml");
        }

        [HttpPost]
        public IActionResult TabloSilOnayla(int id)
        {
            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.Id == id);

            if (tablo == null)
                return NotFound();

            _context.Hucre.RemoveRange(tablo.Hucreler); // önce bağlı hücreleri sil
            _context.FormulTablosu.Remove(tablo);       // sonra tabloyu sil
            _context.SaveChanges();

            TempData["SilmeMesaji"] = "✅ Tablo başarıyla silindi.";
            return RedirectToAction("TabloSil");
        }
        [HttpDelete]
        [Route("api/urun/sil/{id}")]
        public IActionResult UrunSil(int id)
        {
            var urun = _context.Urun
                .Include(u => u.FormulTablolari)
                .FirstOrDefault(u => u.Id == id);

            if (urun == null)
                return NotFound(new { status = "not_found" });

            // Eğer ürünün FormulTablosu varsa ilişkili tablolarla birlikte sil (isteğe bağlı)
            if (urun.FormulTablolari != null && urun.FormulTablolari.Any())
            {
                foreach (var tablo in urun.FormulTablolari)
                {
                    var hucreler = _context.Hucre.Where(h => h.TabloId == tablo.Id);
                    _context.Hucre.RemoveRange(hucreler);
                    _context.FormulTablosu.Remove(tablo);
                }
            }

            _context.Urun.Remove(urun);
            _context.SaveChanges();

            return Ok(new { status = "ok" });
        }


    }
}

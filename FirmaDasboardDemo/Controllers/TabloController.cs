using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Models; // FormulTablosu ve Hucre modeli için gerekli

namespace FirmaDasboardDemo.Controllers
{
    public class TabloController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public TabloController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        [HttpPost("{seoUrl}/api/urun/ekle")]
        public IActionResult UrunEkle(string seoUrl, [FromBody] UrunEkleDto dto)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return Unauthorized();

            bool ayniAdVar = _context.Urun.Any(u => u.FirmaId == firma.Id && u.Ad.ToLower() == dto.Ad.ToLower());
            if (ayniAdVar)
            {
                return Ok(new { status = "duplicate" });
            }

            var urun = new Urun
            {
                Ad = dto.Ad,
                Aciklama = dto.Aciklama,
                FirmaId = firma.Id
            };

            _context.Urun.Add(urun);
            _context.SaveChanges();

            return Ok(new { id = urun.Id, ad = urun.Ad });
        }

       
        [HttpGet("{seoUrl}/api/urun/liste")]
        public IActionResult UrunleriGetir(string seoUrl)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return Unauthorized();

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firma.Id && !_context.FormulTablosu.Any(t => t.UrunId == u.Id))
                .Select(u => new { id = u.Id, ad = u.Ad })
                .ToList();

            return Ok(urunler);
            
        }

        [HttpGet]
        [Route("api/urun/tabloluurun")]
        public IActionResult TablosuOlanUrunleriGetir()
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId && _context.FormulTablosu.Any(t => t.UrunId == u.Id))
                .Select(u => new { id = u.Id, ad = u.Ad })
                .ToList();

            return Ok(urunler);
        }

        [HttpGet("{seoUrl}/Admin/Tablo/TabloOlustur")]
        public IActionResult TabloOlustur(string seoUrl)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return NotFound();

            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            ViewBag.SirketSeoUrl = seoUrl;

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firma.Id)
                .Select(u => new { u.Id, u.Ad })
                .ToList();

            ViewBag.Urunler = urunler;
            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} ADMIN";
            return View("~/Views/Calisan/TabloOlustur.cshtml");
        }

        [HttpPost("{seoUrl}/api/tablo/kaydet")]
        public IActionResult Kaydet(string seoUrl, [FromBody] TabloKayitDto input)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            var calisanId = HttpContext.Session.GetInt32("UserId");

            if (firma == null || calisanId == null)
                return Unauthorized();

            var mevcut = _context.FormulTablosu.Any(t => t.UrunId == input.UrunId);
            if (mevcut)
                return Ok(new { status = "already_exists" });

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
                    GozuksunMu = h.GirdimiYapabilir ? true : h.GozuksunMu,
                    GirdimiYapabilir = h.GirdimiYapabilir
                }).ToList()
            };

            _context.FormulTablosu.Add(tablo);
            _context.SaveChanges();

            return Ok(new { status = "ok", tabloId = tablo.Id });
        }

        [HttpGet("{seoUrl}/Admin/Tablo/TabloDuzenle")]
        public IActionResult TabloDuzenle(string seoUrl)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return NotFound();

            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            ViewBag.SirketSeoUrl = seoUrl;
            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} ADMIN";

            return View("~/Views/Calisan/TabloDuzenle.cshtml");
        }

        [HttpGet]
        public IActionResult VeriGetir(int urunId)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
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

        [HttpGet("{seoUrl}/Admin/Tablo/TabloSil")]
        public IActionResult TabloSil(string seoUrl)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return NotFound();

            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            ViewBag.SirketSeoUrl = seoUrl;

            var tablolar = _context.FormulTablosu
                .Include(t => t.Urun)
                .Where(t => t.Urun.FirmaId == firma.Id)
                .Select(t => new {
                    Id = t.Id,
                    Ad = t.Ad,
                    UrunAd = t.Urun.Ad
                })
                .ToList();

            ViewBag.Tablolar = tablolar;
            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} ADMIN";
            return View("~/Views/Calisan/TabloSil.cshtml");
        }

        [HttpPost]
        [Route("{firmaSeoUrl}/Tablo/TabloSilOnayla/{id}")]
        public IActionResult TabloSilOnayla(string firmaSeoUrl, int id)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.Id == id);

            if (tablo == null)
                return Json(new { status = "error", message = "Tablo bulunamadı." });

            _context.Hucre.RemoveRange(tablo.Hucreler);
            _context.FormulTablosu.Remove(tablo);
            _context.SaveChanges();

            return Json(new { status = "ok", message = "Tablo başarıyla silindi." });
        }



        [HttpDelete("{seoUrl}/api/urun/sil/{id}")]
        public IActionResult UrunSil(string seoUrl, int id)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null)
                return Unauthorized();

            var urun = _context.Urun
                .Include(u => u.FormulTablolari)
                .FirstOrDefault(u => u.Id == id && u.FirmaId == firma.Id);

            if (urun == null)
                return NotFound(new { status = "not_found" });

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



        [HttpGet("{seoUrl}/api/firma-degiskenleri")]
        public IActionResult FirmaDegiskenleriniGetir(string seoUrl)
        {
            if (HttpContext.Session.GetInt32("CalisanId") == null)
                return RedirectToAction("Login", "Calisan", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == seoUrl);
            if (firma == null) return Unauthorized();

            var degiskenler = _context.FirmaDegiskenler
                .Where(fd => fd.FirmaId == firma.Id)
                .Select(fd => new {
                    Ad = fd.Ad,
                    Deger = fd.Deger
                })
                .ToList();

            return Ok(degiskenler);
        }

    }



}

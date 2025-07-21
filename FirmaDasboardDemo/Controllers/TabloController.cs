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

            if (string.IsNullOrWhiteSpace(input.TabloAdi) || input.Hucreler == null || !input.Hucreler.Any())
                return BadRequest(new { status = "invalid_data", message = "Eksik veri gönderildi." });

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
                    IsFormul = h.IsFormul
                }).ToList()
            };

            _context.FormulTablosu.Add(tablo);
            _context.SaveChanges();

            return Ok(new { status = "ok", tabloId = tablo.Id });
        }
    }
}

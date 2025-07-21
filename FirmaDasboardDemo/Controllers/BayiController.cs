using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FirmaDashboardDemo.Controllers
{
    public class BayiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BayiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📄 Bayi Listesi Sayfası
        [HttpGet]
        public IActionResult BayiList()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return RedirectToAction("Login", "Calisan");

            var bayiler = _context.BayiFirmalari
                .Include(bf => bf.Bayi)
                .Where(bf => bf.FirmaId == firmaId)
                .Select(bf => bf.Bayi)
                .ToList();

            return View(bayiler);
        }

        // 📦 Bayi listesini JSON olarak getir
        [HttpGet]
        public IActionResult GetBayiler()
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var bayiler = _context.BayiFirmalari
                .Include(bf => bf.Bayi)
                .Where(bf => bf.FirmaId == firmaId)
                .Select(bf => new
                {
                    bf.Bayi.Id,
                    bf.Bayi.Ad,
                    bf.Bayi.Adres,
                    bf.Bayi.Il,
                    bf.Bayi.Ilce,
                    bf.Bayi.Email,
                    bf.Bayi.Telefon,
                    bf.Bayi.AktifMi
                }).ToList();

            return Json(bayiler);
        }

        // ➕ Form üzerinden yeni bayi ekle (modal submit)
        [HttpPost]
        public IActionResult AddBayi(Bayi model)
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var bayiRol = _context.Roller.FirstOrDefault(r => r.Ad == "Bayi");
            if (bayiRol == null)
                return BadRequest(new { status = "role_not_found" });

            model.Sifre = "1234"; // default şifre
            model.AktifMi = true;
            model.RolId = bayiRol.Id;

            _context.Bayiler.Add(model);
            _context.SaveChanges();

            _context.BayiFirmalari.Add(new BayiFirma
            {
                BayiId = model.Id,
                FirmaId = firmaId.Value
            });
            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        // 🔁 JSON listesiyle toplu ekleme/güncelleme (örneğin JSpreadsheet için)
        [HttpPost]
        public IActionResult SaveBayiler([FromBody] JsonElement data)
        {
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return Unauthorized();

            var list = JsonSerializer.Deserialize<List<Bayi>>(data.GetRawText());
            if (list == null || list.Count == 0)
                return BadRequest(new { status = "empty_list" });

            foreach (var item in list)
            {
                if (item.Id > 0)
                {
                    var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == item.Id);
                    if (bayi != null)
                    {
                        bayi.Ad = item.Ad;
                        bayi.Adres = item.Adres;
                        bayi.Il = item.Il;
                        bayi.Ilce = item.Ilce;
                        bayi.Email = item.Email;
                        bayi.Telefon = item.Telefon;
                        bayi.AktifMi = item.AktifMi;
                    }
                }
                else
                {
                    var bayiRol = _context.Roller.FirstOrDefault(r => r.Ad == "Bayi");
                    if (bayiRol == null)
                        return BadRequest(new { status = "role_not_found" });

                    var yeniBayi = new Bayi
                    {
                        Ad = item.Ad,
                        Adres = item.Adres,
                        Il = item.Il,
                        Ilce = item.Ilce,
                        Email = item.Email,
                        Telefon = item.Telefon,
                        Sifre = "1234",
                        AktifMi = item.AktifMi,
                        RolId = bayiRol.Id
                    };

                    _context.Bayiler.Add(yeniBayi);
                    _context.SaveChanges();

                    _context.BayiFirmalari.Add(new BayiFirma
                    {
                        BayiId = yeniBayi.Id,
                        FirmaId = firmaId.Value
                    });
                }
            }

            _context.SaveChanges();
            return Ok(new { status = "success" });
        }

        // ❌ Bayi sil
        [HttpPost]
        public IActionResult DeleteBayi(int id)
        {
            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == id);
            if (bayi != null)
            {
                _context.Bayiler.Remove(bayi);

                var bayiFirmalar = _context.BayiFirmalari
                    .Where(x => x.BayiId == id).ToList();

                _context.BayiFirmalari.RemoveRange(bayiFirmalar);
                _context.SaveChanges();

                return Ok(new { status = "deleted" });
            }

            return NotFound(new { status = "not_found" });
        }

        // 🔍 Düzenleme için ID ile bayi verisi getir
        [HttpGet]
        public IActionResult GetBayiById(int id)
        {
            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == id);
            if (bayi == null) return NotFound();

            return Json(bayi);
        }

        // 📝 Güncelleme (modal form ile)
        [HttpPost]
        public IActionResult UpdateBayi([FromBody] Bayi model)
        {
            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == model.Id);
            if (bayi == null) return NotFound();

            bayi.Ad = model.Ad;
            bayi.Adres = model.Adres;
            bayi.Il = model.Il;
            bayi.Ilce = model.Ilce;
            bayi.Email = model.Email;
            bayi.Telefon = model.Telefon;
            bayi.AktifMi = model.AktifMi;

            _context.SaveChanges();
            return Ok(new { status = "success" });
        }
    }
}

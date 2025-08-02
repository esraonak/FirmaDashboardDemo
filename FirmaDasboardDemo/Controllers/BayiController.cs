using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FirmaDasboardDemo.Controllers;

namespace FirmaDashboardDemo.Controllers
{
    public class BayiController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public BayiController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        private bool FirmaSeoUrlGecerliMi(string firmaSeoUrl)
        {
            var sessionSeo = HttpContext.Session.GetString("FirmaSeoUrl");
            return !string.IsNullOrEmpty(sessionSeo) && sessionSeo.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("{firmaSeoUrl}/Admin/Bayi/BayiList")]
        public IActionResult BayiList(string firmaSeoUrl)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");

            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
            var firma = _context.Firmalar.FirstOrDefault(x => x.Id == firmaId);
            if (firma != null)
                ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} BAYİ PANELİ";

            return View();
        }

        [HttpGet("{firmaSeoUrl}/Admin/Bayi/GetBayiler")]
        public IActionResult GetBayiler(string firmaSeoUrl)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

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

        [HttpPost("{firmaSeoUrl}/Admin/Bayi/AddBayi")]
        public IActionResult AddBayi(string firmaSeoUrl, Bayi model)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            // ✅ Aynı email varsa uyarı dön
            bool emailExists = _context.Bayiler.Any(b => b.Email == model.Email)
                || _context.Firmalar.Any(f => f.Email == model.Email)
                || _context.FirmaCalisanlari.Any(c => c.Email == model.Email);

            if (emailExists)
            {
                return Json(new { status = "email_exists" });
            }

            var bayiRol = _context.Roller.FirstOrDefault(r => r.Ad == "Bayi");
            if (bayiRol == null)
                return BadRequest(new { status = "role_not_found" });

            model.Sifre = "1234"; // default şifre
            model.AktifMi = true;
            model.RolId = bayiRol.Id;

            _context.Bayiler.Add(model);
            _context.SaveChanges();

            // BayiFirma ilişkisi kaydediliyor
            _context.BayiFirmalari.Add(new BayiFirma
            {
                BayiId = model.Id,
                FirmaId = firmaId.Value
            });
            _context.SaveChanges();

            return Json(new { status = "success" });
        }


        [HttpPost("{firmaSeoUrl}/Admin/Bayi/SaveBayiler")]
        public IActionResult SaveBayiler(string firmaSeoUrl, [FromBody] JsonElement data)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

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

        [HttpPost("{firmaSeoUrl}/Admin/Bayi/DeleteBayi")]
        public IActionResult DeleteBayi(string firmaSeoUrl, int id)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == id);
            if (bayi != null)
            {
                _context.Bayiler.Remove(bayi);

                var bayiFirmalar = _context.BayiFirmalari.Where(x => x.BayiId == id).ToList();
                _context.BayiFirmalari.RemoveRange(bayiFirmalar);

                _context.SaveChanges();
                return Ok(new { status = "deleted" });
            }

            return NotFound(new { status = "not_found" });
        }

        [HttpGet("{firmaSeoUrl}/Admin/Bayi/GetBayiById")]
        public IActionResult GetBayiById(string firmaSeoUrl, int id)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == id);
            if (bayi == null) return NotFound();

            return Json(bayi);
        }

        [HttpPost("{firmaSeoUrl}/Admin/Bayi/UpdateBayi")]
        public IActionResult UpdateBayi(string firmaSeoUrl, [FromBody] Bayi model)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();

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

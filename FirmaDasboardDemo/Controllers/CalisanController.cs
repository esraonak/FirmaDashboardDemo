using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace FirmaDashboardDemo.Controllers
{
    public class CalisanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalisanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            var calisan = _context.FirmaCalisanlari
                .FirstOrDefault(x => x.Email == Username && x.Sifre == Password && x.AktifMi);

            if (calisan != null)
            {
                HttpContext.Session.SetString("UserRole", "Calisan");
                HttpContext.Session.SetInt32("UserId", calisan.Id);
                HttpContext.Session.SetInt32("FirmaId", calisan.FirmaId);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction("BayiList", "Bayi");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ Tüm çalışanları getir (JSON)
        [HttpGet]
        public IActionResult Calisanlar()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCalisanlar()
        {
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

        // ✅ Yeni çalışan ekle
        [HttpPost]
        public IActionResult AddCalisan(FirmaCalisani model)
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null) return Unauthorized();
            var calisanrol = _context.Roller.FirstOrDefault(r => r.Ad == "Calisan");
            if (calisanrol == null)
                return BadRequest(new { status = "role_not_found" });
            model.FirmaId = firmaId.Value;
            model.Sifre = "1234"; // default şifre
            model.AktifMi = true;
            model.RolId = calisanrol.Id;
            _context.FirmaCalisanlari.Add(model);
            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        // ✅ Güncelleme
        [HttpPost]
        public IActionResult UpdateCalisan([FromBody] FirmaCalisani model)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == model.Id);
            if (calisan == null)
                return NotFound();

            calisan.AdSoyad = model.AdSoyad;
            calisan.Email = model.Email;
            calisan.Telefon = model.Telefon;
            calisan.AktifMi = model.AktifMi;

            _context.SaveChanges();

            return Json(new { status = "success" });
        }

        // ✅ Silme
        [HttpPost]
        public IActionResult DeleteCalisan(int id)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(x => x.Id == id);
            if (calisan == null)
                return NotFound();

            _context.FirmaCalisanlari.Remove(calisan);
            _context.SaveChanges();

            return Json(new { status = "deleted" });
        }

        // ✅ ID ile çalışan getir
        [HttpGet]
        public IActionResult GetCalisanById(int id)
        {
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
    }
}

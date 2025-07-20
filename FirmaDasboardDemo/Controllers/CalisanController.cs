using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using System.Linq;

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
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

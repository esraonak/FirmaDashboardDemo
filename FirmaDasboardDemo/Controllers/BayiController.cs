using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using System.Linq;

namespace FirmaDashboardDemo.Controllers
{
    public class BayiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BayiController(ApplicationDbContext context)
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
            var bayi = _context.Bayiler
                .FirstOrDefault(x => x.Email == Username && x.Sifre == Password && x.AktifMi);

            if (bayi != null)
            {
                HttpContext.Session.SetString("UserRole", "Bayi");
                HttpContext.Session.SetInt32("UserId", bayi.Id);
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

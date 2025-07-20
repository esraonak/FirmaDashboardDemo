using Microsoft.AspNetCore.Mvc;

namespace FirmaDashboardDemo.Controllers
{
    public class AdminController : Controller
    {
        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            // Demo amaçlı sabit kullanıcı kontrolü (veritabanı eklenince değiştirilecek)
            if (Username == "admin" && Password == "123")
            {
                // Oturum oluşturma vs. ileride eklenebilir
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }


    }
}

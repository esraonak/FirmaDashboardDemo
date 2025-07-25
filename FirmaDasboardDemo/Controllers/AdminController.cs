﻿using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using System.Linq;

namespace FirmaDashboardDemo.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
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
            var admin = _context.FirmaCalisanlari
                .FirstOrDefault(x => x.Email == Username && x.Sifre == Password && x.AktifMi);

            if (admin != null)
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetInt32("UserId", admin.Id);
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

using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class SiteController : Controller
{
    private readonly IConfiguration _config;

    public SiteController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("Site/DemoTalep")]
    [ValidateAntiForgeryToken]
    public IActionResult DemoTalep(string AdSoyad, string Email, string FirmaAdi, string Not, string Telefon)
    {
        string body = $@"
📩 Yeni Demo Talebi

👤 Ad Soyad: {AdSoyad}
📧 E-posta: {Email}
🏢 Firma Adı: {FirmaAdi}
📞 Telefon: {Telefon}
📝 Not: {Not}
        ";

        try
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]);
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, "TenteCRM Demo Talebi"),
                Subject = "Yeni Demo Talebi",
                Body = body,
                IsBodyHtml = false // çünkü HTML değil, metin format
            };

            mail.To.Add("fatihonak@gmail.com");

            client.Send(mail);

            TempData["Mesaj"] = "✅ Demo talebiniz başarıyla gönderildi. En kısa sürede sizinle iletişime geçeceğiz.";
        }
        catch (Exception ex)
        {
            TempData["Hata"] = "❌ Mail gönderilirken bir hata oluştu: " + ex.Message;
        }

        return RedirectToAction("Index");
    }
}

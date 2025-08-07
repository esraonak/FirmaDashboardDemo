using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using System.Net.Mail;
using System.Net;
using System.Text;
using FirmaDasboardDemo.Helpers;

namespace FirmaDasboardDemo.Controllers
{
    [Route("{firmaSeoUrl}/Kullanici")]
    public class KullaniciController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KullaniciController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Şifremi Unuttum Sayfası (GET)
        [HttpGet("SifremiUnuttum")]
        public IActionResult SifremiUnuttum(string firmaSeoUrl)
        {
            ViewBag.FirmaSeoUrl = firmaSeoUrl;
            return View();
        }

        // Şifre Sıfırlama İşlemi (POST)
        [HttpPost("SifremiUnuttum")]
        public IActionResult SifremiUnuttum(string firmaSeoUrl, string email)
        {
            var calisan = _context.FirmaCalisanlari.FirstOrDefault(c => c.Email == email);
            var bayi = _context.Bayiler.FirstOrDefault(b => b.Email == email);

            if (calisan == null && bayi == null)
            {
                TempData["Mesaj"] = "Bu e-posta adresi sistemde kayıtlı değil. Lütfen yöneticinizle iletişime geçin.";
                return RedirectToAction("SifremiUnuttum", new { firmaSeoUrl });
            }

            // 1. Yeni şifre oluştur
            string yeniSifre = SifreUret(8);

            try
            {
                // 2. Önce mail gönderilmeye çalışılır
                MailGonder(email, yeniSifre);

                // 3. Mail başarılıysa şifre güncellenir
                if (calisan != null)
                    calisan.Sifre = HashHelper.Hash(yeniSifre);

                else if (bayi != null)
                    bayi.Sifre = HashHelper.Hash(yeniSifre);


                _context.SaveChanges(); // 4. En son DB'ye kaydedilir

                TempData["Mesaj"] = "Yeni şifreniz e-posta adresinize gönderildi.";
            }
            catch (Exception ex)
            {
                // ⚠️ Hata durumunda şifre değişmez
                TempData["Mesaj"] = "Şifre gönderilemedi. Lütfen yöneticinizle iletişime geçin.";
            }

            return RedirectToAction("SifremiUnuttum", new { firmaSeoUrl });
        }


        // Yardımcı: Güçlü rastgele şifre üret
        private string SifreUret(int uzunluk)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, uzunluk)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Yardımcı: SMTP ile mail gönder
        private void MailGonder(string aliciMail, string yeniSifre)
        {
            string gonderen = "noreply@tentecrm.com.tr"; // kendi adresin
            string sifre = "mailSifren";                 // kendi SMTP şifren
            string konu = "Yeni Şifreniz - TenteCRM";
            string icerik = $@"
                <p>Merhaba,</p>
                <p>Yeni şifreniz: <b>{yeniSifre}</b></p>
                <p>Lütfen giriş yaptıktan sonra şifrenizi değiştirin.</p>
                <p><i>TenteCRM Otomasyon Sistemi</i></p>";

            var mail = new MailMessage();
            mail.From = new MailAddress(gonderen, "TenteCRM");
            mail.To.Add(aliciMail);
            mail.Subject = konu;
            mail.Body = icerik;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient("smtp.yourdomain.com", 587)
            {
                Credentials = new NetworkCredential(gonderen, sifre),
                EnableSsl = true
            };

            smtp.Send(mail);
        }
    }
}

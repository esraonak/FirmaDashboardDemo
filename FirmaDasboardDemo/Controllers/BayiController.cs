using Microsoft.AspNetCore.Mvc;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FirmaDasboardDemo.Controllers;
using FirmaDasboardDemo.Helpers;
namespace FirmaDashboardDemo.Controllers
{
    public class BayiController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public BayiController(
         ApplicationDbContext context,
         IConfiguration configuration
     ) : base(context)
        {
            _context = context;
            _configuration = configuration;
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
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
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
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
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
            try
            {
                // 0) Oturum ve routing doğrulama
                int? calisanId = HttpContext.Session.GetInt32("CalisanId");
                int? firmaId = HttpContext.Session.GetInt32("FirmaId");
                if (calisanId == null || firmaId == null)
                    return Redirect($"/{firmaSeoUrl}/Admin/Login");

                var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId.Value);
                if (firma == null || !string.Equals(firma.SeoUrl, firmaSeoUrl, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { status = "firma_not_found" });

                // 1) Lisans bayi limiti
                int mevcutBayiSayisi = _context.BayiFirmalari.Count(bf => bf.FirmaId == firma.Id);
                if (mevcutBayiSayisi >= firma.MaxBayiSayisi)
                    return Json(new { status = "max_bayi_limit" });

                // 2) Email tekillik (firma scope’unda)
                bool emailExists =
                    _context.Bayiler.Any(b => b.Email == model.Email && b.BayiFirmalari.Any(bf => bf.FirmaId == firma.Id)) ||
                    _context.FirmaCalisanlari.Any(c => c.Email == model.Email && c.FirmaId == firma.Id) ||
                    string.Equals(firma.Email, model.Email, StringComparison.OrdinalIgnoreCase);

                if (emailExists)
                    return Json(new { status = "email_exists" });

                // 3) Rol
                var bayiRol = _context.Roller.FirstOrDefault(r => r.Ad == "Bayi");
                if (bayiRol == null)
                    return BadRequest(new { status = "role_not_found" });

                // 4) Rastgele şifre + login URL (BaseUrl appsettings/secrets/env)
                string plainPassword = SifreUretici.RastgeleSifreUret(10);
                string baseUrl = _configuration["Sistem:BaseUrl"] ?? "https://tentecrm.com.tr";
                string loginUrl = $"{baseUrl}/{firma.SeoUrl}/Bayi/Login";

                // 5) Bayiye e-posta (başarısızsa kayıt yapma)
                string konu = "TenteCRM Bayi Giriş Bilgileri";
                string icerik =
                    $"Merhaba {model.Ad},\n\n" +
                    $"TenteCRM bayi paneline giriş bilgileriniz:\n\n" +
                    $"Giriş Adresi: {loginUrl}\n" +
                    $"E-posta: {model.Email}\n" +
                    $"Şifre: {plainPassword}\n\n" +
                    $"Giriş yaptıktan sonra şifrenizi değiştirmenizi öneririz.\nİyi çalışmalar.";

                var (ok, err) = MailHelper.MailGonderDetay(model.Email, konu, icerik);
                if (!ok)
                {
                    // SMTP hatasını logla
                    _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
                    {
                        KullaniciRol = HttpContext.Session.GetString("UserRole") ?? "Calisan",
                        KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "Bilinmiyor",
                        FirmaSeo = firma.SeoUrl,
                        Url = HttpContext.Request?.Path.Value,
                        Tarih = DateTime.Now,
                        HataMesaji = "SMTP hata: " + (err ?? "-"),
                        StackTrace = err
                    });
                    _context.SaveChanges();

                    return Json(new { status = "email_send_fail", message = err ?? "E-posta gönderilemedi." });
                }

                // 6) Şifreyi hash’le, bayi kaydı + ilişki
                model.Sifre = HashHelper.Hash(plainPassword);
                model.AktifMi = true;
                model.RolId = bayiRol.Id;

                _context.Bayiler.Add(model);
                _context.SaveChanges();

                _context.BayiFirmalari.Add(new BayiFirma
                {
                    BayiId = model.Id,
                    FirmaId = firma.Id
                });
                _context.SaveChanges();

                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                // Genel log
                try
                {
                    _context.SuperAdminHataKayitlari.Add(new SuperAdminHataKaydi
                    {
                        KullaniciRol = HttpContext.Session.GetString("UserRole") ?? "Calisan",
                        KullaniciAdi = HttpContext.Session.GetString("UserAd") ?? "Bilinmiyor",
                        FirmaSeo = firmaSeoUrl,
                        Url = HttpContext.Request?.Path.Value,
                        Tarih = DateTime.Now,
                        HataMesaji = ex.Message,
                        StackTrace = ex.ToString()
                    });
                    _context.SaveChanges();
                }
                catch { /* yut */ }

                return Json(new { status = "error", message = "Bayi eklenirken beklenmeyen bir hata oluştu." });
            }
        }



        [HttpPost("{firmaSeoUrl}/Admin/Bayi/SaveBayiler")]
        public IActionResult SaveBayiler(string firmaSeoUrl, [FromBody] JsonElement data)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
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
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
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
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == id);
            if (bayi == null) return NotFound();

            return Json(bayi);
        }

        [HttpPost("{firmaSeoUrl}/Admin/Bayi/UpdateBayi")]
        public IActionResult UpdateBayi(string firmaSeoUrl, [FromBody] Bayi model)
        {
            if (!FirmaSeoUrlGecerliMi(firmaSeoUrl))
                return Unauthorized();
            int? calisanId = HttpContext.Session.GetInt32("CalisanId");
            if (calisanId == null)
                return Redirect("/" + firmaSeoUrl + "/Admin/Login");
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var bayi = _context.Bayiler.FirstOrDefault(x => x.Id == model.Id);
            if (bayi == null) return NotFound();

            // 🔁 E-posta başka kullanıcıya ait mi? (Kendisi hariç)
            bool emailExists =
                _context.Bayiler
                    .Any(b => b.Email == model.Email &&
                              b.Id != model.Id &&
                              b.BayiFirmalari.Any(bf => bf.FirmaId == firmaId)) ||

                _context.FirmaCalisanlari
                    .Any(c => c.Email == model.Email &&
                              c.FirmaId == firmaId) ||

                _context.Firmalar
                    .Any(f => f.Id == firmaId && f.Email == model.Email);

            if (emailExists)
            {
                return Json(new { status = "email_exists" });
            }

            // 🔄 Güncelleme işlemi
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

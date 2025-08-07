using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using FirmaDasboardDemo.Dtos;
using FirmaDasboardDemo.Controllers;
using Newtonsoft.Json;
using FirmaDasboardDemo.Enums;
using FirmaDasboardDemo.Helpers;


namespace FirmaDashboardDemo.Controllers
{
    [Route("{firmaSeoUrl}/Bayi")]
    public class BayiSayfasiController : BaseBayiController
    {
        private readonly ApplicationDbContext _context;

        public BayiSayfasiController(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        [HttpGet("Login")]
        public IActionResult Login(string firmaSeoUrl)
        {
            if (string.IsNullOrWhiteSpace(firmaSeoUrl))
                return Content("Eksik bağlantı.");

            var firma = _context.Firmalar
                .AsEnumerable()
                .FirstOrDefault(f => f.SeoUrl.Equals(firmaSeoUrl, StringComparison.OrdinalIgnoreCase));

            if (firma == null || !firma.AktifMi)
            {
                TempData["LoginError"] = "Lisans süreniz dolmuştur. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Login", new { firmaSeoUrl }); // 🔁 recursive redirect, doğru parametreyle
            }

            // ✅ Session’a firma bilgilerini ve kullanıcı rolünü yaz
            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("FirmaLogo", firma.LogoUrl ?? ""); // 🔁 logo destekli
            HttpContext.Session.SetString("UserRole", "Bayi");
            HttpContext.Session.SetString("Instagram", firma.InstagramUrl ?? "");
            HttpContext.Session.SetString("Twitter", firma.TwitterUrl ?? "");
            HttpContext.Session.SetString("Facebook", firma.FacebookUrl ?? "");
            HttpContext.Session.SetString("WebSitesi", firma.WebSitesi ?? "");

            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} BAYİ PANELİ";

            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login(string Username, string Password)
        {
            string girilenHash = HashHelper.Hash(Password); // ✅ Şifre hashlenir

            var bayi = _context.Bayiler.FirstOrDefault(b => b.Email == Username && b.Sifre == girilenHash && b.AktifMi);
            if (bayi == null)
            {
                TempData["LoginError"] = "Geçersiz kullanıcı adı veya şifre";
                return RedirectToAction("Login", new { firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl") });
            }

            // 🔐 FirmaSeoUrl session’dan alınmalı
            var firmaSeoUrl = HttpContext.Session.GetString("FirmaSeoUrl");
            if (string.IsNullOrEmpty(firmaSeoUrl))
                return Content("Firma bilgisi eksik.");

            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == firmaSeoUrl && f.AktifMi);
            if (firma == null)
                return Content("Firma bulunamadı.");

            var eslesmeVarMi = _context.BayiFirmalari
                .Any(bf => bf.BayiId == bayi.Id && bf.FirmaId == firma.Id);
            if (!eslesmeVarMi)
                return Content("Bu firmayla ilişkilendirilmiş bayi hesabı bulunamadı.");

            // Session kayıtları
            HttpContext.Session.SetInt32("UserId", bayi.Id);
            HttpContext.Session.SetInt32("RolId", bayi.RolId);
            HttpContext.Session.SetString("UserAd", bayi.Ad);
            HttpContext.Session.SetString("UserRole", "Bayi");
            HttpContext.Session.SetInt32("BayiId", bayi.Id);

            HttpContext.Session.SetInt32("FirmaId", firma.Id);
            HttpContext.Session.SetString("FirmaSeoUrl", firma.SeoUrl);
            HttpContext.Session.SetString("FirmaAd", firma.Ad);
            HttpContext.Session.SetString("FirmaLogo", firma.LogoUrl ?? "");

            HttpContext.Session.SetString("Instagram", firma.InstagramUrl ?? "");
            HttpContext.Session.SetString("Twitter", firma.TwitterUrl ?? "");
            HttpContext.Session.SetString("Facebook", firma.FacebookUrl ?? "");
            HttpContext.Session.SetString("WebSitesi", firma.WebSitesi ?? "");

            if (!bayi.KvkkOnaylandiMi || !bayi.EtkOnaylandiMi)
            {
                return RedirectToAction("OnayFormu", "BayiSayfasi", new { firmaSeoUrl = firma.SeoUrl });
            }

            return Redirect("/" + firma.SeoUrl + "/Bayi/Dashboard");
        }

        [HttpGet("Logout")]
        public IActionResult Logout(string firmaSeoUrl)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", new { firmaSeoUrl });
        }



        [HttpGet("Dashboard")]
        public IActionResult Dashboard(string firmaSeoUrl)
        {
            var bayiId = HttpContext.Session.GetInt32("UserId");
            if (bayiId == null)
                return RedirectToAction("Login", new { firmaSeoUrl });

            ViewBag.PanelBaslik = $"{firmaSeoUrl.ToUpper()} BAYİ PANELİ";
            return View("BayiDashboard");
        }



        // ✅ Seçilen firmaya ait ürünleri getirir
        [HttpGet("BayiSayfasi/GetUrunler")]
        public IActionResult GetUrunler()
        {
            var firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (firmaId == null)
                return Unauthorized();

            var urunler = _context.Urun
                .Where(u => u.FirmaId == firmaId)
                .Where(u => _context.FormulTablosu.Any(ft => ft.UrunId == u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    ad = u.Ad
                })
                .ToList();

            return Json(urunler);
        }


        // ✅ Seçilen ürünün bayi tarafından görüntülenebilir formül tablosunu getirir
        [HttpGet("BayiSayfasi/GetTablo")]
        public IActionResult GetTablo(int urunId)
        {
            var tablo = _context.FormulTablosu
                .Include(t => t.Hucreler)
                .FirstOrDefault(t => t.UrunId == urunId);

            if (tablo == null)
                return NotFound();

            // ❗️TÜM hücreleri alıyoruz artık
            var hucreListesi = tablo.Hucreler.ToList();

            // Satır/sütun sayısını tüm hücrelere göre ayarla
            int maxRow = hucreListesi
                .Select(h => int.Parse(System.Text.RegularExpressions.Regex.Match(h.HucreAdi, @"\d+").Value))
                .DefaultIfEmpty(0)
                .Max();

            int maxCol = hucreListesi
                .Select(h => h.HucreAdi[0] - 'A' + 1)
                .DefaultIfEmpty(0)
                .Max();

            var hucreMap = hucreListesi.ToDictionary(h => h.HucreAdi, h => h);

            // Grid oluştur
            var grid = new List<List<string>>();
            for (int r = 0; r < maxRow; r++)
            {
                var row = new List<string>();
                for (int c = 0; c < maxCol; c++)
                {
                    var hucreAdi = $"{(char)('A' + c)}{r + 1}";
                    if (hucreMap.TryGetValue(hucreAdi, out var hucre))
                        row.Add(hucre.Formul);
                    else
                        row.Add("");
                }
                grid.Add(row);
            }

            return Json(new
            {
                hucreler = hucreListesi.Select(h => new
                {
                    hucreAdi = h.HucreAdi,
                    gozuksunMu = h.GozuksunMu,
                    girdimiYapabilir = h.GirdimiYapabilir,
                    formulMu = h.IsFormul,
                    satisFiyatMi = h.SatisFiyatMi
                }),
                grid
            });
        }
        [HttpGet("KullaniciAyar")]
        public IActionResult KullaniciAyar()
        {

            return View();
        }

        [HttpPost("KullaniciAyar")]
        public IActionResult KullaniciAyarGuncelle(string EskiSifre, string YeniSifre, string YeniSifreTekrar)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            if (bayiId == null)
                return RedirectToAction("Login");

            var bayi = _context.Bayiler.Find(bayiId);
            if (bayi == null)
            {
                TempData["Error"] = "Bayi bulunamadı.";
                return RedirectToAction("KullaniciAyar");
            }

            if (bayi.Sifre != EskiSifre)
            {
                TempData["Error"] = "Mevcut şifre hatalı.";
                return RedirectToAction("KullaniciAyar");
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                TempData["Error"] = "Yeni şifreler uyuşmuyor.";
                return RedirectToAction("KullaniciAyar");
            }

            bayi.Sifre = YeniSifre;
            _context.SaveChanges();

            TempData["Success"] = "Şifre başarıyla güncellendi.";
            return RedirectToAction("KullaniciAyar");
        }


        [HttpGet("OnayFormu")]
        public IActionResult OnayFormu(string firmaSeoUrl)
        {
            ViewBag.FirmaSeoUrl = firmaSeoUrl;
            return View();
        }

        [HttpPost("OnayFormu")]
        [ValidateAntiForgeryToken]
        public IActionResult OnayFormu(string firmaSeoUrl, bool KvkkOnaylandiMi, bool EtkOnaylandiMi)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "BayiSayfasi", new { firmaSeoUrl });

            var bayi = _context.Bayiler.FirstOrDefault(b => b.Id == userId);
            if (bayi == null)
            {
                TempData["Error"] = "Oturum geçersiz.";
                return RedirectToAction("Login", new { firmaSeoUrl });
            }


            bayi.KvkkOnaylandiMi = KvkkOnaylandiMi;
            bayi.EtkOnaylandiMi = EtkOnaylandiMi;

            _context.SaveChanges();

            return Redirect("/" + firmaSeoUrl + "/Bayi/Dashboard");
        }

        // BayiSayfasiController.cs

        [HttpGet("BayiMesaj")]
        public IActionResult BayiMesaj()
        {
            var viewModel = new BayiMesajGonderDto
            {
                UrunId = 0,
                Mesaj = "",
                GirilenHucreler = new List<HucreDegeri>(),
                GorunenHucreler = new List<HucreDegeri>(),
                SatisFiyatlari = new List<HucreDegeri>()
            };

            return View(viewModel);
        }

        [HttpPost("BayiMesajGonder")]
        public IActionResult BayiMesajGonder(
            string firmaSeoUrl,
            [FromForm] int UrunId,
            [FromForm] string Mesaj,
            [FromForm] string MesajTuru,
            [FromForm] string GirilenHucreler,
            [FromForm] string GorunenHucreler,
            [FromForm] string SatisFiyatlari)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");

            if (bayiId == null || firmaId == null)
                return Unauthorized();

            if (!Enum.TryParse<BayiMesajTuru>(MesajTuru, out var mesajTuruEnum))
            {
                TempData["Error"] = "Mesaj türü geçersiz.";
                return Redirect("/" + firmaSeoUrl + "/Bayi/BayiMesaj");
            }

            // ÜRÜN VAR MI kontrolü (HesaplamaSonucu için)
            if (mesajTuruEnum == BayiMesajTuru.HesaplamaSonucu)
            {
                bool urunVarMi = _context.Urun.Any(u => u.Id == UrunId && u.FirmaId == firmaId);
                if (!urunVarMi)
                {
                    TempData["Error"] = "Geçersiz ürün bilgisi.";
                    return Redirect("/" + firmaSeoUrl + "/Bayi/BayiMesaj");
                }
            }

            // Mesaj BAŞLIĞI (ticket) oluştur
            var bayiMesaj = new BayiMesaj
            {
                BayiId = bayiId.Value,
                FirmaId = firmaId.Value,
                UrunId = mesajTuruEnum == BayiMesajTuru.HesaplamaSonucu ? UrunId : null,
                Tarih = DateTime.Now,
                GirilenHucrelerJson = JsonConvert.SerializeObject(
                    mesajTuruEnum == BayiMesajTuru.HesaplamaSonucu ?
                        JsonConvert.DeserializeObject<List<HucreDegeri>>(GirilenHucreler) :
                        new List<HucreDegeri>()),
                GorunenHucrelerJson = JsonConvert.SerializeObject(
                    mesajTuruEnum == BayiMesajTuru.HesaplamaSonucu ?
                        JsonConvert.DeserializeObject<List<HucreDegeri>>(GorunenHucreler) :
                        new List<HucreDegeri>()),
                SatisFiyatlariJson = JsonConvert.SerializeObject(
                    mesajTuruEnum == BayiMesajTuru.HesaplamaSonucu ?
                        JsonConvert.DeserializeObject<List<HucreDegeri>>(SatisFiyatlari) :
                        new List<HucreDegeri>()),
                AktifMi = true,
                BayiGoruntulediMi = true,
                FirmaGoruntulediMi = false,
                MesajTuru = mesajTuruEnum
            };

            _context.BayiMesajlar.Add(bayiMesaj);
            _context.SaveChanges(); // ID gerekiyor

            // Ilk mesaj satırı
            var mesajSatiri = new MesajSatiri
            {
                BayiMesajId = bayiMesaj.Id, // ✅ foreign key bağlantısı burada
                BayiId = bayiId.Value,
                FirmaId = firmaId.Value,
                UrunId = bayiMesaj.UrunId,
                Icerik = Mesaj?.Trim(),
                GonderenFirmaMi = false,
                Tarih = DateTime.Now,
                OkunduMu = false
            };

            _context.MesajSatirlari.Add(mesajSatiri);
            _context.SaveChanges();

            TempData["Success"] = "Mesajınız firmaya başarıyla iletildi.";
            return Redirect("/" + firmaSeoUrl + "/Bayi/BayiMesaj");
        }



        [HttpGet("GetSonHesaplama")]
        public IActionResult GetSonHesaplama(string firmaSeoUrl)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");
            if (bayiId == null || firmaId == null)
                return Unauthorized();

            var kayit = _context.BayiHesaplamaKayitlari
                .Where(x => x.BayiId == bayiId && x.FirmaId == firmaId)
                .OrderByDescending(x => x.Tarih)
                .FirstOrDefault();

            if (kayit == null)
                return NotFound();

            var urunAdi = _context.Urun.FirstOrDefault(x => x.Id == kayit.UrunId)?.Ad ?? "Bilinmiyor";

            var dto = new BayiMesajVeriDto
            {
                UrunAdi = urunAdi,
                GirilenHucreler = JsonConvert.DeserializeObject<List<HucreDegeri>>(kayit.GirilenHucrelerJson ?? "[]"),
                GorunenHucreler = JsonConvert.DeserializeObject<List<HucreDegeri>>(kayit.GorunenHucrelerJson ?? "[]"),
                SatisFiyatlari = JsonConvert.DeserializeObject<List<HucreDegeri>>(kayit.SatisFiyatlariJson ?? "[]")
            };

            return Ok(dto);
        }

        [HttpGet("Mesajlarim")]
        public IActionResult Mesajlarim(string firmaSeoUrl)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            if (bayiId == null) return RedirectToAction("Login");

            var mesajlar = _context.BayiMesajlar
                .Include(x => x.Urun)
                .Where(x => x.BayiId == bayiId)
                .OrderByDescending(x => x.Tarih)
                .ToList();

            ViewBag.FirmaSeoUrl = firmaSeoUrl;
            return View("Mesajlarim", mesajlar);
        }

        [HttpGet("BayiMesajDetay/{id}")]
        public IActionResult BayiMesajDetay(string firmaSeoUrl, int id)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            if (bayiId == null)
                return RedirectToAction("Login", "Bayi", new { firmaSeoUrl });

            var firma = _context.Firmalar.FirstOrDefault(f => f.SeoUrl == firmaSeoUrl);
            if (firma == null)
                return RedirectToAction("BayiMesaj", new { firmaSeoUrl });

            var mesaj = _context.BayiMesajlar
                .Include(m => m.Mesajlar.OrderBy(m => m.Tarih))
                .Include(m => m.Urun)
                .FirstOrDefault(m => m.Id == id && m.BayiId == bayiId && m.FirmaId == firma.Id);

            if (mesaj == null)
            {
                TempData["Hata"] = "Mesaj bulunamadı.";
                return RedirectToAction("BayiMesaj", new { firmaSeoUrl });
            }

            // Firma mesajlarını okunmuş işaretle
            foreach (var satir in mesaj.Mesajlar.Where(m => m.GonderenFirmaMi && !m.OkunduMu))
                satir.OkunduMu = true;

            mesaj.BayiGoruntulediMi = true;
            _context.SaveChanges();

            ViewBag.FirmaSeoUrl = firmaSeoUrl;
            return View("~/Views/BayiSayfasi/BayiMesajDetay.cshtml", mesaj);
        }

        [HttpPost("MesajaCevapEkle")]
        public IActionResult MesajaCevapEkle(string firmaSeoUrl, int BayiMesajId, string Icerik)
        {
            int? bayiId = HttpContext.Session.GetInt32("BayiId");
            int? firmaId = HttpContext.Session.GetInt32("FirmaId");

            if (bayiId == null || firmaId == null || string.IsNullOrWhiteSpace(Icerik))
                return Unauthorized();

            var mesaj = _context.BayiMesajlar
                .FirstOrDefault(m => m.Id == BayiMesajId && m.BayiId == bayiId && m.AktifMi);

            if (mesaj == null)
                return NotFound();

            var yeniSatir = new MesajSatiri
            {
                BayiMesajId = BayiMesajId,
                BayiId = bayiId.Value,
                FirmaId = firmaId.Value,
                Icerik = Icerik.Trim(),
                GonderenFirmaMi = false,
                Tarih = DateTime.Now,
                OkunduMu = false
            };

            _context.MesajSatirlari.Add(yeniSatir);
            _context.SaveChanges();

            return Redirect("/" + firmaSeoUrl + "/Bayi/BayiMesajDetay/" + BayiMesajId);
        }


    }
}

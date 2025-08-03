using FirmaDasboardDemo.Data;
using FirmaDasboardDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

[Route("{firmaSeoUrl}/Admin/Degisken")]
public class DegiskenController : Controller
{
    private readonly ApplicationDbContext _context;

    public DegiskenController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("FirmaDegiskenleri")]
    public IActionResult FirmaDegiskenleri(string firmaSeoUrl)
    {
        var firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (firmaId == null) return Unauthorized();
        return View();
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll(string firmaSeoUrl)
    {
        var firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (firmaId == null) return Unauthorized();

        var veriler = _context.FirmaDegiskenler
            .Where(x => x.FirmaId == firmaId)
            .Select(x => new
            {
                id = x.Id,
                ad = x.Ad,
                aciklama = x.Aciklama,
                deger = x.Deger
            })
            .ToList();

        return Json(new { data = veriler });
    }

    [HttpPost("Ekle")]
    public IActionResult Ekle(string firmaSeoUrl, string Ad, string? Aciklama, string Deger)
    {
        int? calisanId = HttpContext.Session.GetInt32("CalisanId");
        if (calisanId == null)
            return Unauthorized();
        var firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (firmaId == null) return Unauthorized();

        // Deger parse et
        if (!float.TryParse(Deger, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedDeger))
        {
            return Json(new { status = "invalidDeger" });
        }

        // Aynı ad var mı?
        bool adVarMi = _context.FirmaDegiskenler.Any(fd => fd.FirmaId == firmaId && fd.Ad.ToLower() == Ad.ToLower());
        if (adVarMi)
        {
            return Json(new { status = "duplicate" });
        }

        var degisken = new FirmaDegisken
        {
            Ad = Ad,
            Aciklama = Aciklama,
            Deger = parsedDeger,
            FirmaId = firmaId.Value
        };

        _context.FirmaDegiskenler.Add(degisken);
        _context.SaveChanges();

        return Json(new { status = "success" });
    }

    [HttpPost("Sil/{id}")]
    public IActionResult Sil(string firmaSeoUrl, int id)
    {
        int? calisanId = HttpContext.Session.GetInt32("CalisanId");
        if (calisanId == null)
            return Unauthorized();
        var degisken = _context.FirmaDegiskenler.FirstOrDefault(d => d.Id == id);
        if (degisken == null)
            return NotFound();

        _context.FirmaDegiskenler.Remove(degisken);
        _context.SaveChanges();

        return Json(new { status = "success" });
    }

    [HttpGet("GetById/{id}")]
    public IActionResult GetById(int id)
    {
        int? calisanId = HttpContext.Session.GetInt32("CalisanId");
        if (calisanId == null)
            return Unauthorized();
        int? firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (firmaId == null) return Unauthorized();

        var degisken = _context.FirmaDegiskenler.FirstOrDefault(d => d.Id == id && d.FirmaId == firmaId);
        if (degisken == null) return NotFound();

        return Json(new
        {
            id = degisken.Id,
            ad = degisken.Ad,
            aciklama = degisken.Aciklama,
            deger = degisken.Deger.ToString(CultureInfo.InvariantCulture)
        });
    }

    [HttpPost("Guncelle")]
    public IActionResult Guncelle(string firmaSeoUrl, int Id, string Ad, string? Aciklama, string Deger)
    {
        int? calisanId = HttpContext.Session.GetInt32("CalisanId");
        if (calisanId == null)
            return Unauthorized();
        int? firmaId = HttpContext.Session.GetInt32("FirmaId");
        if (firmaId == null) return Unauthorized();

        if (!float.TryParse(Deger, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedDeger))
        {
            return Json(new { status = "invalidDeger" });
        }

        var mevcut = _context.FirmaDegiskenler
            .FirstOrDefault(x => x.Id == Id && x.FirmaId == firmaId);

        if (mevcut == null)
            return Json(new { status = "notfound" });

        mevcut.Ad = Ad;
        mevcut.Aciklama = Aciklama;
        mevcut.Deger = parsedDeger;

        try
        {
            _context.SaveChanges();
            return Json(new { status = "success" });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Json(new { status = "concurrency" });
        }
        catch (Exception)
        {
            return Json(new { status = "error" });
        }
    }
}

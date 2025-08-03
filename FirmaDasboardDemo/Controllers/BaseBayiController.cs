using FirmaDasboardDemo.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

public class BaseBayiController : Controller
{
    private readonly ApplicationDbContext _context;

    public BaseBayiController(ApplicationDbContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var action = context.RouteData.Values["action"]?.ToString()?.ToLower();
        var controller = context.RouteData.Values["controller"]?.ToString()?.ToLower();

        // 🟡 Login ve Logout sayfaları için doğrulama gerekmez
        if (action == "login" || action == "logout" || action == "onayformu")
        {
            base.OnActionExecuting(context);
            return;
        }

        // 🛑 Rol ve Firma kontrolü
        var userRole = context.HttpContext.Session.GetString("UserRole");
        var firmaId = context.HttpContext.Session.GetInt32("FirmaId");

        if (userRole != "Bayi" || firmaId == null)
        {
            var seo = context.HttpContext.Session.GetString("FirmaSeoUrl") ?? "tente";
            context.Result = new RedirectToActionResult("Login", "BayiSayfasi", new { firmaSeoUrl = seo });
            return;
        }

        // 🏢 Firma bilgilerini çek
        var firma = _context.Firmalar.FirstOrDefault(f => f.Id == firmaId.Value);
        if (firma != null)
        {
            ViewBag.FirmaLogo = firma.LogoUrl;
            ViewBag.FirmaAd = firma.Ad;
            ViewBag.FirmaSeoUrl = firma.SeoUrl;
            ViewBag.UserRole = userRole;
            ViewBag.PanelBaslik = $"{firma.SeoUrl.ToUpper()} BAYİ PANELİ";
        }

        base.OnActionExecuting(context);
    }
}

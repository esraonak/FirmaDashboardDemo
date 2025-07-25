using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FirmaDasboardDemo.Controllers
{
    public class BaseBayiController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var routeData = context.RouteData;
            var action = routeData.Values["action"]?.ToString()?.ToLower();
            var controller = routeData.Values["controller"]?.ToString()?.ToLower();

            // Bayi login/logout gibi giriş sayfaları kontrol dışı bırakılır
            if (action == "login" || action == "logout")
            {
                base.OnActionExecuting(context);
                return;
            }

            var userRole = context.HttpContext.Session.GetString("UserRole");
            var firmaSeoUrl = context.HttpContext.Session.GetString("FirmaSeoUrl") ?? "tente";

            if (userRole != "Bayi" || string.IsNullOrEmpty(firmaSeoUrl))
            {
                context.Result = new RedirectToActionResult("Login", "BayiSayfasi", new { firmaSeoUrl });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

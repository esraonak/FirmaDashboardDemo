using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FirmaDasboardDemo.Controllers
{
    public class BaseSuperAdminController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"]?.ToString()?.ToLower();
            var controller = context.RouteData.Values["controller"]?.ToString()?.ToLower();

            // Giriş ve çıkış action'ları kontrol dışı
            if (action == "login" || action == "logout")
            {
                base.OnActionExecuting(context);
                return;
            }

            var rol = context.HttpContext.Session.GetString("Rol");

            if (rol != "SuperAdmin")
            {
                context.Result = new RedirectToActionResult("Login", "SuperAdmin", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

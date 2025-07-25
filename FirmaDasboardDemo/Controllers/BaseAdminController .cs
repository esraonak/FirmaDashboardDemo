using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class BaseAdminController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var routeData = context.RouteData;
        var action = routeData.Values["action"]?.ToString()?.ToLower();
        var controller = routeData.Values["controller"]?.ToString()?.ToLower();

        // Login ve Logout işlemlerine müdahale etme
        if (action == "login" || action == "logout")
        {
            base.OnActionExecuting(context);
            return;
        }

        var userRole = context.HttpContext.Session.GetString("UserRole");
        var firmaSeoUrl = context.HttpContext.Session.GetString("FirmaSeoUrl") ?? "TENTECI";

        if (userRole != "Calisan")
        {
            context.Result = new RedirectToActionResult("Login", "Calisan", new { firmaSeoUrl });
            return;
        }

        base.OnActionExecuting(context);
    }

}

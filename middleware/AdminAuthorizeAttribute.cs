using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TechNova.middleware
{

    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }

}
